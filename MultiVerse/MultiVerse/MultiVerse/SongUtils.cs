using System;
using System.Collections.Generic;
using fftwlib;
using System.Runtime.InteropServices;

namespace MultiVerse
{
	class SongUtils
	{
		const float hamming_alpha = 0.54f;
		public const int FFTW_WINDOW_SIZE = 1024;
		const int THRESHOLD_WINDOW_SIZE = 10;
		const float THRESHOLD_MULTIPLIER = 1.5f;

		IntPtr ptrIn, ptrOut;
		IntPtr FFTW_plan;

		FMOD.System FMOD_system = null;
		FMOD.Sound FMOD_sound = null;
		FMOD.Channel FMOD_channel = null;

		string loadedMusicFile = "";

		uint lengthInMs = 0;
		float frequency = 0;
		int numChannels = 0;
		int byteDepth = 0;

		static readonly SongUtils instance = new SongUtils();
		public static SongUtils Instance
		{
			get
			{
				return instance;
			}
		}

		SongUtils()
		{
			// creating an(there can be more) FMOD system object with a maximum of 32 channels playing simultaneously
			FMOD.Factory.System_Create(ref FMOD_system);
			FMOD_system.init(32, FMOD.INITFLAGS.NORMAL, (IntPtr)null);
		}

		~SongUtils()
		{
			if (FMOD_sound != null)
			{
				FMOD_sound.release();
			}

			if (FMOD_system != null)
			{
				FMOD_system.close();
				FMOD_system.release();
			}
		}

		void LoadSong(string musicFile)
		{

			if (FMOD_sound != null || musicFile != loadedMusicFile)
			{
				if (FMOD_sound != null)
				{
					FMOD_sound.release();
				}

				FMOD_system.createStream(musicFile, FMOD.MODE.SOFTWARE | FMOD.MODE._2D, ref FMOD_sound);

				loadedMusicFile = musicFile;
			}
		}

		public void PlaySong(string musicFile)
		{
			LoadSong(musicFile);

			FMOD_system.playSound(FMOD.CHANNELINDEX.FREE, FMOD_sound, false, ref FMOD_channel);
		}

		public void SetPaused(bool pause)
		{
			if (FMOD_channel != null)
				FMOD_channel.setPaused(pause);
		}

		public void ReleaseSong()
		{
			if (FMOD_sound != null)
			{
				FMOD_sound.release();
				FMOD_sound = null;
				loadedMusicFile = "";
			}
		}

		public uint GetLengthInMs(string musicFile)
		{
			uint length = 0;

			LoadSong(musicFile);

			FMOD_sound.getLength(ref length, FMOD.TIMEUNIT.MS);

			return length;
		}

		public float GetFrequency(string musicFile)
		{
			float freq = 0;

			LoadSong(musicFile);

			{
				float dummy_volume = 0;
				float dummy_pan = 0;
				int dummy_priority = 0;
				FMOD_sound.getDefaults(ref freq, ref dummy_volume, ref dummy_pan, ref dummy_priority);
			}

			return freq;
		}

		public void PerformBeatDetection(string musicFile, bool playOnFinish, ref float[] prunnedSpectralFlux, ref float[] beatTimes)
		{
			LoadSong(musicFile);

			lengthInMs = 0;
			FMOD_sound.getLength(ref lengthInMs, FMOD.TIMEUNIT.MS);

			frequency = GetFrequency(musicFile);

			{
				FMOD.SOUND_TYPE dummy_type = 0;
				FMOD.SOUND_FORMAT dummy_format = 0;
				FMOD_sound.getFormat(ref dummy_type, ref dummy_format, ref numChannels, ref byteDepth);
				byteDepth /= 8;
			}

			FMOD_sound.seekData(0);

			// complex
			ptrIn = fftwf.malloc(FFTW_WINDOW_SIZE * byteDepth * numChannels * 2);
			ptrOut = fftwf.malloc(FFTW_WINDOW_SIZE * byteDepth * numChannels * 2);

			FFTW_plan = fftwf.dft_1d(FFTW_WINDOW_SIZE, ptrIn, ptrOut, fftw_direction.Forward, fftw_flags.Estimate);

			if (byteDepth != 2)
			{
				Console.WriteLine("ERROR: files with bit depth other than 16 are not supported - Terminating");
				Environment.Exit(-1);
			}

			long numOutSamples = (long)Math.Ceiling(frequency * ((float)lengthInMs / 1000) / FFTW_WINDOW_SIZE);
			float[] spectralFlux = new float[numOutSamples];

			float[] hamming_window = new float[FFTW_WINDOW_SIZE];
			for (int i = 0; i < hamming_window.Length; ++i)
			{
				hamming_window[i] = hamming_alpha - (1.0f - hamming_alpha) * (float)Math.Cos((2.0f * Math.PI * i)
					/ (FFTW_WINDOW_SIZE - 1.0f));
			}

			int fftw_in_bytesize = FFTW_WINDOW_SIZE * byteDepth * numChannels;

			float[] fIn = new float[FFTW_WINDOW_SIZE];
			float[] fOut = new float[FFTW_WINDOW_SIZE / 2 + 1];
			float[] last_fout = new float[FFTW_WINDOW_SIZE / 2 + 1];

			uint bufferSize = (uint)((frequency * numChannels * byteDepth) * ((float)lengthInMs / 1000) + 1);
			IntPtr buffer = Marshal.AllocHGlobal((int)bufferSize);

			uint bytesRead = 0;

			FMOD_sound.readData(buffer, bufferSize, ref bytesRead);

			long samples = bytesRead / (byteDepth * numChannels);

			// 16 bites audio fájloknál !!
			Int16[] intIn = new Int16[FFTW_WINDOW_SIZE * numChannels];

			int bufferSeekPos = 0;

			int counter = 0;
			while (samples >= FFTW_WINDOW_SIZE)
			{
				// átmásolunk FFTW_WINDOW_SIZE darabnyi sample-t egy integer tömbbe ahol
				// numChannel-enként vannak az új sample-ök
				Marshal.Copy(buffer + bufferSeekPos,
					intIn,
					0,
					(int)FFTW_WINDOW_SIZE * (int)numChannels);

				// keeping track
				bufferSeekPos += fftw_in_bytesize;
				samples -= FFTW_WINDOW_SIZE;

				// az ideiglenes integer tömbből feltöltjük az FFTW által használt float input tömböt
				int sum = 0;
				for (int i = 0; i != FFTW_WINDOW_SIZE; i++)
				{
					// nemfoglalkozunk külön a sávokkal, vesszük az átlaguk
					sum = 0;
					for (int j = 0; j != numChannels; j++)
					{
						sum += intIn[i * 2 + j];
					}

					// normalizáljuk (32768 <= 16bites hangfájlok, előjeles integer)
					//	ha negatív akkor '-32767'-el kéne osztani
					// a hamming window értékeit is itt számítjuk be ..google it
					fIn[i] = ((sum / numChannels) / /*32768f*/65535f) * hamming_window[i];
				}

				// mivel az FFTW unmanaged memóriával dolgozik ezért odavissza kell másolgatni
				//	nekünk a memória mutatókkal elég kényelmetlen lenne dolgozni
				Marshal.Copy(fIn, 0, ptrIn, FFTW_WINDOW_SIZE);
				fftwf.execute(FFTW_plan);
				Marshal.Copy(ptrOut, fOut, 0, FFTW_WINDOW_SIZE / 2 + 1);

				// ezen ponton az 'fOut' tömb FFTW_WINDOW_SIZE darab sample frekvencia képét tartalmazza
				//	összesen 'FFTW_WINDOW_SIZE / 2 + 1' frekvencia sávra felosztva

				// a 'spectral flux' az az egyes frekvencia sávok változásai két sample window közt
				// itt fontos megjegyezni, hogy több 'spectralFlux'-hez hasonló tömböt is használhatnánk
				//	ha egyes frekencia tartományokra külön szeretnénk vizsgálni az értékeket (pl.: magas furulya hangok)
				float flux = 0;
				for (int i = 0; i < fOut.Length; i++)
				{
					//float value = (fOut[i] - last_fout[i]);
					float value = (Math.Abs(fOut[i]) - Math.Abs(last_fout[i]));

					// a negatív értékeket nem vesszük figyelembe, csak a kiugrások érdekelnek
					flux += value < 0 ? 0 : value;
				}
				spectralFlux[counter] = flux;

				for (int i = 0; i < fOut.Length; i++)
				{
					last_fout[i] = fOut[i];
				}

				counter++;
			}

			// felállítunk egy határértékét ami mindig az átlag 'spectral flux'
			//	azaz az átlag változások a frekvenciában
			// ezt megkell szorozni egy 1-nél nagyobb értékkel mert ami alatta van az kilesz dobva
			float[] threshold = new float[numOutSamples];

			for (int i = 0; i < spectralFlux.Length; i++)
			{
				int start = Math.Max(0, i - THRESHOLD_WINDOW_SIZE);
				int end = Math.Min(spectralFlux.Length - 1, i + THRESHOLD_WINDOW_SIZE);
				float mean = 0;
				for (int j = start; j <= end; j++)
					mean += spectralFlux[j];
				mean /= (end - start);
				threshold[i] = mean * THRESHOLD_MULTIPLIER;
			}

			if (prunnedSpectralFlux != null)
			{
				// minden ami a threshold alatt van azt kidobjuk
				prunnedSpectralFlux = new float[numOutSamples];

				for (int i = 0; i < spectralFlux.Length; i++)
				{
					if (threshold[i] <= spectralFlux[i])
						prunnedSpectralFlux[i] = spectralFlux[i] - threshold[i];
					else
						prunnedSpectralFlux[i] = 0;
				}
			}

			if (beatTimes != null)
			{
				// mennyi időnek felel meg egy sample
				float sampleLengthInMs = (float)lengthInMs / numOutSamples;
				List<float> beatStamps = new List<float>();
				for (int i = 0; i < spectralFlux.Length; i++)
				{
					if (threshold[i] <= spectralFlux[i])
					{
						beatStamps.Add(i * sampleLengthInMs);
						spectralFlux[i] = spectralFlux[i] - threshold[i];
					}
					else
					{
						spectralFlux[i] = 0;
					}
				}

				beatTimes = new float[beatStamps.Count];
				beatTimes = beatStamps.ToArray();
			}

			if (playOnFinish)
			{
				PlaySong(musicFile);
			}
			else
			{
				FMOD_sound.release();
				FMOD_sound = null;
				loadedMusicFile = "";
			}

			fftwf.free(ptrIn);
			fftwf.free(ptrOut);
			fftwf.destroy_plan(FFTW_plan);
			Marshal.FreeHGlobal(buffer);
		}

		void FMOD_RESULTCHECK(FMOD.RESULT result)
		{
			if (result != FMOD.RESULT.OK)
			{
				Console.WriteLine("FMOD error! " + result + " - " + FMOD.Error.String(result));
				Environment.Exit(-1);
			}
		}
	}
}

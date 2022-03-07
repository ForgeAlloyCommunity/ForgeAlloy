
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forge.Networking.Messaging
{
	public class RateTracker
	{

		public int averageSamples = 50;

		public int LastAverageValue;

		private int currentAverageSamples;
		public float currentAverageRaw;

		public bool IsTracking = false;

		public bool trackSampling = false;
		private DateTime lastUpdate;
		private RateTracker sampling;

		public RateTracker(int samples, bool tracking)
		{
			Init(0, samples, tracking);
		}

		public RateTracker(float startValue, int samples, bool tracking)
		{
			Init(startValue, samples, tracking);
		}


		public void Init(float startValue, int samples, bool tracking)
		{
			currentAverageRaw = startValue;
			LastAverageValue = (int)startValue;
			averageSamples = samples;

			// Track stats on UpdateStats
			if (tracking)
			{
				trackSampling = true;
				sampling = new RateTracker(samples, false);
				lastUpdate = DateTime.Now;
			}
		}


		public void UpdateStats(long elapsedMs)
		{
			IsTracking = true;
			float LastValue = (float)elapsedMs;

			currentAverageSamples++;
			if (currentAverageSamples > averageSamples && averageSamples != 0)
			{
				currentAverageRaw += (LastValue - currentAverageRaw) / (averageSamples + 1);
			}
			else
			{
				currentAverageRaw += (LastValue - currentAverageRaw) / currentAverageSamples;
			}

			int currentAverageRounded = (int)Math.Floor(currentAverageRaw);

			if (LastAverageValue != currentAverageRounded)
			{
				LastAverageValue = currentAverageRounded;
			}

			// Track how often stats are updated
			if (trackSampling)
			{
				sampling.UpdateStats((DateTime.Now - lastUpdate).Milliseconds);
			}
		}

		public void Reset()
		{
			lastUpdate = DateTime.Now;
			currentAverageRaw = 0;
			currentAverageSamples = 0;
			LastAverageValue = 0;
			if (trackSampling)
				sampling.Reset();
		}

	}
}

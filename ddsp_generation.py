'''

Links and documentation:
 - https://magenta.tensorflow.org/ddsp

'''

# Ignore a bunch of deprecation warnings
import warnings
warnings.filterwarnings("ignore")

import ddsp
import ddsp.training
import numpy as np
import tensorflow as tf
import io
from scipy.io import wavfile

from datetime import datetime

def create_audio_file(array_of_floats):
    if len(array_of_floats.shape) == 2:
        array_of_floats = array_of_floats[0]
    normalizer = float(np.iinfo(np.int16).max)
    array_of_ints = np.array(
        np.asarray(array_of_floats) * normalizer, dtype=np.int16)
    memfile = io.BytesIO()
    wavfile.write(fileName, sample_rate, array_of_ints)
    memfile.close()


sample_rate = 16000
n_frames = 1000
hop_size = 64
n_samples = n_frames * hop_size

# Create a synthesizer object.
additive_synth = ddsp.synths.Additive(n_samples=n_samples, sample_rate=sample_rate, name='additive_synth')

# Generate some arbitrary inputs.

# Amplitude [batch, n_frames, 1].
# Make amplitude linearly decay over time.
amps = np.linspace(1.0, -3.0, n_frames)
amps = amps[np.newaxis, :, np.newaxis]

# Harmonic Distribution [batch, n_frames, n_harmonics].
# Make harmonics decrease linearly with frequency.
n_harmonics = 30
harmonic_distribution = (np.linspace(-2.0, 2.0, n_frames)[:, np.newaxis] + 
                         np.linspace(3.0, -3.0, n_harmonics)[np.newaxis, :])
harmonic_distribution = harmonic_distribution[np.newaxis, :, :]

# Fundamental frequency in Hz [batch, n_frames, 1].
f0_hz = 440.0 * np.ones([1, n_frames, 1], dtype=np.float32)

## Some weird control envelopes...

# Amplitude [batch, n_frames, 1].
amps = np.ones([n_frames]) * -5.0
amps[:50] +=  np.linspace(0, 7.0, 50)
amps[50:200] += 7.0
amps[200:900] += (7.0 - np.linspace(0.0, 7.0, 700))
amps *= np.abs(np.cos(np.linspace(0, 2*np.pi * 10.0, n_frames)))
amps = amps[np.newaxis, :, np.newaxis]

# Harmonic Distribution [batch, n_frames, n_harmonics].
n_harmonics = 20
harmonic_distribution = np.ones([n_frames, 1]) * np.linspace(1.0, -1.0, n_harmonics)[np.newaxis, :]
for i in range(n_harmonics):
  harmonic_distribution[:, i] = 1.0 - np.linspace(i * 0.09, 2.0, 1000)
  harmonic_distribution[:, i] *= 5.0 * np.abs(np.cos(np.linspace(0, 2*np.pi * 0.1 * i, n_frames)))
  if i % 2 != 0:
    harmonic_distribution[:, i] = -3
harmonic_distribution = harmonic_distribution[np.newaxis, :, :]

# Fundamental frequency in Hz [batch, n_frames, 1].
f0_hz = np.ones([n_frames]) * 200.0
f0_hz[:100] *= np.linspace(2, 1, 100)**2
f0_hz[200:1000] += 20 * np.sin(np.linspace(0, 8.0, 800) * 2 * np.pi * np.linspace(0, 1.0, 800))  * np.linspace(0, 1.0, 800)
f0_hz = f0_hz[np.newaxis, :, np.newaxis]

# Get valid controls
controls = additive_synth.get_controls(amps, harmonic_distribution, f0_hz)

audio = additive_synth.get_signal(**controls)

# create_audio_file(audio)


def sin_phase(mod_rate):
  """Helper function."""
  # n_samples = audio.size
  n_seconds = n_samples / sample_rate
  phase = tf.sin(tf.linspace(0.0, mod_rate * n_seconds * 2.0 * np.pi, n_samples))
  return phase[tf.newaxis, :, tf.newaxis]

def modulate_audio(audio, center_ms, depth_ms, mod_rate):
  mod_delay = ddsp.effects.ModDelay(center_ms=center_ms,
                                    depth_ms=depth_ms,
                                    gain_scale_fn=None,
                                    phase_scale_fn=None)

  phase = sin_phase(mod_rate)  # Hz
  gain = 1.0 * np.ones_like(audio)[..., np.newaxis]
  audio_out = 0.5 * mod_delay(audio, gain, phase)

  # Listen.
  create_audio_file(audio_out)

# Three different effects.
print('Flanger')
fileName = 'Flanger.wav'
modulate_audio(audio, center_ms=0.75, depth_ms=0.75, mod_rate=0.25)

print('Chorus')
fileName = 'Chorus.wav'
modulate_audio(audio, center_ms=25.0, depth_ms=1.0, mod_rate=2.0)

print('Vibrato')
fileName = 'Vibrato.wav'
modulate_audio(audio, center_ms=25.0, depth_ms=12.5, mod_rate=5.0)


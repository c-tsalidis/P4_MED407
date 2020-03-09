import numpy as np
import io
from scipy.io import wavfile

def create_audio_file(array_of_floats):
    print(len(array_of_floats))
    if array_of_floats.shape != None:
        if len(array_of_floats.shape) == 2:
            array_of_floats = array_of_floats[0]
    normalizer = float(np.iinfo(np.int16).max)
    array_of_ints = np.array(
        np.asarray(array_of_floats) * normalizer, dtype=np.int16)
    memfile = io.BytesIO()
    wavfile.write(fileName, samplingFreq, array_of_ints)
    memfile.close()

fileName = "dsp_sound.wav"

samplingFreq, fullSignal = wavfile.read('Chorus.wav')
print(samplingFreq)
print("----------------------------")
fullSignal = fullSignal/2**15 # normalise
print(fullSignal)
create_audio_file(fullSignal)


guitarFreqs = [32.70, 34.65, 36.71, 38.89, 41.20, 43.65, 46.25, 49, 51.91, 55, 58.27, 61.74]
guitarFreqsExpanded = np.array([None] * 64000)
for x in range(64000):
    guitarFreqsExpanded[x] = float(guitarFreqs[x % len(guitarFreqs)])

create_audio_file(guitarFreqsExpanded)

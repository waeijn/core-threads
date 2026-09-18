from pydub import AudioSegment
try:
    sound = AudioSegment.from_mp3('Assets/_Project/Audio/Others/ButtonClick.mp3')
    print('Success')
except Exception as e:
    print(f'Error: {e}')

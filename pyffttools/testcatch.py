from catchbuilder import CatchBuilder
import numpy as np
import cv2

img = cv2.imread('~\\..\\1268634163_01.jpg')
pat = cv2.imread('~\\..\\1268634163_02.jpg')
image = np.asarray(img, float)/255
pattern = np.asarray(pat, float)/255
builder = CatchBuilder(pattern)
data = builder.Catch(image)
y, x = builder.ArgMax(data)
height, width, channels = pattern.shape
cv2.rectangle(img, (x, y), (x+width, y+height), (0, 0, 255), 1)
print(x, y)
cv2.imshow('image', img)
cv2.imshow('pattern', pat)
cv2.waitKey(0)
cv2.destroyAllWindows()

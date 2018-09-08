from polarlogarithmbuilder import PolarLogarithmBuilder
import numpy as np
import cv2

img = cv2.imread('~\\..\\Physicians.jpg')
height, width, channels = img.shape
data = np.asarray(img, float)/255
builder = PolarLogarithmBuilder(256, 256, 1.03, 0)
y0, x0 = builder.Center(data)
cv2.imshow('image', builder.PolarLogarithm(data, y0, x0)[::-1])
cv2.waitKey(0)
cv2.destroyAllWindows()

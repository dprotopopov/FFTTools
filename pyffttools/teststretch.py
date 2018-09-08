from stretchbuilder import StretchBuilder
import numpy as np
import cv2

img = cv2.imread('~\\..\\Physicians.jpg')
height, width, channels = img.shape
data = np.asarray(img, float)/255
builder1 = StretchBuilder((int)(height*2), (int)(width*2))
builder2 = StretchBuilder((int)(height/2), (int)(width/2))
cv2.imshow('inc', builder1.Stretch(data))
cv2.imshow('dec', builder2.Stretch(data))
cv2.waitKey(0)
cv2.destroyAllWindows()

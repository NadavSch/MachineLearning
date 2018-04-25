import numpy as np
import sys
import math
from random import shuffle
import matplotlib.pyplot as plt

w = [1,1,1]
b = [1,1,1]
def normpdf(x, mean, sd):
    var = float(sd)**2
    pi = 3.1415926
    denom = (2*pi*var)**.5
    num = math.exp(-(float(x)-float(mean))**2/(2*var))
    return num/denom

def getEstimatedP(x):
    e1 = np.exp(w[0]*x+b[0])
    e2 = np.exp(w[1]*x+b[1])
    e3 = np.exp(w[2]*x+b[2])
    se = e1+e2+e3
    return e1/se

def getNormalP(x):
    n1 = normpdf(x,2,1)
    n2 = normpdf(x,4,1)
    n3 = normpdf(x,6,1)
    ne = n1+n2+n3
    return n1/ne

def predict(x):
    e1 = np.exp(w[0]*x+b[0])
    e2 = np.exp(w[1]*x+b[1])
    e3 = np.exp(w[2]*x+b[2])
    se = e1+e2+e3    
    if e1/se > e2/se and e1/se > e3/se:
        return 1
    elif e2/se > e1/se and e2/se > e3/se:
        return 2
    else:
        return 3

def updateVector(x, y):
    rate = 0.05
    se = np.exp(w[0]*x+b[0]) + np.exp(w[1]*x+b[1]) + np.exp(w[2]*x+b[2])
    for i in range(0, 2):
        c = 0
        if y == i+1:
            c = 1
        w[i] = w[i] - rate*(np.exp(w[i]*x+b[i])/se-c)*x
        b[i] = b[i] - rate*(np.exp(w[i]*x+b[i])/se-c)

def getEstimatedPList(x):
    predictL = []
    for i in range(0, len(x)):
        predictL.append(getEstimatedP(x[i]))
    return predictL

def getNormalPList(x):
    normalL = []
    for i in range(0, len(x)):
        normalL.append(getNormalP(x[i]))
    return normalL
    
#init data
s1 = np.random.normal(2,1,100)
s2 = np.random.normal(4,1,100)
s3 = np.random.normal(6,1,100)
data = []
for i in range(0,len(s1)):
    data.append([s1[i],1])
for i in range(0,len(s2)):
    data.append([s2[i],2])
for i in range(0,len(s3)):
    data.append([s3[i],3])

#for each epoch
for i in range(0,200):
    shuffle(data)
    #loop on the data
    for j in range(0,len(data)):
        x = data[j][0]
        y = data[j][1]     
        y_hat = predict(x)
        updateVector(x,y)
#plot the probability of the estimated value and the normal value
x = np.arange(0, 10, 0.1)
plt.plot(x, getEstimatedPList(x), 'r')
plt.plot(x, getNormalPList(x),'b')
plt.xlabel('Red - Estimated probability | Blue - Normal Distribution probability')
plt.show()

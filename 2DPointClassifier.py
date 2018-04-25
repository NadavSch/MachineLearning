import numpy as np
import matplotlib.pyplot as plt
from random import shuffle

def predict(W,B,X):
    A = X.dot(W)+B
    expA = np.exp(A)
    Y = expA / sum(expA)
    return Y
    
def train(W,B,X,Y,Data):
    rate = 0.05
    for k in range(0,len(Data)):
        i = Data[k]
        se = np.exp(X[i].dot(W)+B)
        for j in range(0,3):
            c = 0
            if Y[i] == j:
                c = 1
            W[0][j] = W[0][j] - rate*(se[j]/sum(se)-c)*X[i][0]
            W[1][j] = W[1][j] - rate*(se[j]/sum(se)-c)*X[i][1]
            B[j] = B[j] - rate*(se[j]/sum(se)-c)
            B[j] = B[j] - rate*(se[j]/sum(se)-c)

DataLen = 500
X1 = np.random.randn(DataLen,2) + np.array([7,7])
X2 = np.random.randn(DataLen,2) + np.array([0,0])
X3 = np.random.randn(DataLen,2) + np.array([5,2])

X = np.vstack([X1,X2,X3])
Y = np.array([0]*DataLen+[1]*DataLen+[2]*DataLen)
D = []
for i in range(0, len(X)):
    D.append([X[i],Y[i]])

Data = []
for i in range(DataLen*3):
    Data.append(i)

W = np.random.randn(2,3)
B = np.random.randn(3)
for i in range(0, 100):
    shuffle(Data)
    train(W,B,X,Y,Data)
    

print predict(W,B,np.array([-2,3]))


plt.scatter(X[:,0],X[:,1],c=Y,s=100)
plt.show()
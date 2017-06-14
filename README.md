# ANT
Machine Learning plugin for Rhino\grasshopper based on Python\ [scikit-learn](http://scikit-learn.org/) module.
![scikit_grass](https://cloud.githubusercontent.com/assets/6969514/26666295/73118c52-469f-11e7-9c9b-b2f44c41ab3a.png)

## Goal.
This project aims to develop a machine learning plugin for Rhino\Grasshopper by making use of the well-known Python module (Scikit-learn) using Rhnio-Common C# and python programming language to bring sophisticated Machine-Learning supervised and unsupervised learning methods to be handy to regular designers and architects. It is open source and is released under BSD "simplified" licence.

## Instructions for installing
This plugin is based on Python and scikit-learn, while scikit-learn depends on Numpy and scipy modules. Thus, it is important to ensure having these tools installed in your system and working properly.
To install scikit-learn on your machine, you could follow instructions on this link: [installing scikit-learn](http://scikit-learn.org/stable/install.html).

#### To install scikit-learn, It is highly recommended that you do this via python bundle, this comes with all required libraries, such as: [Python(x,y)](http://python-xy.github.io/), [Anaconda](https://www.continuum.io/downloads) and [Canopy](https://www.enthought.com/products/canopy/)

After installing Python, numPy, SciPy, matplotlib and scikit-learn, Open grasshopper and drag and drop ANT.gha into grasshopper canvas, you will notice a new tap "ANT" has appeared. Then, initiate ANT component to make sure everything is working properly.

![plugnplay](https://cloud.githubusercontent.com/assets/6969514/26765670/03332d34-4981-11e7-9778-d209cf9b3bcd.jpg)

On the other hand, this plugin (in the present time) has been tested in Windows environment with python 2.7. If you are using earlier of latter versions, we highly recommend that you kindly submit any bug appears. 

## How to ANT ?

1. Create your own data CSV file as shown in the figure below, the first line consists of the features' labels, while the last column is the targets and each line represents a data instance used for trainine. 

2. Add new CSV dataset component, Open your csv file using Openfile component, then, link data and targets from csv dataset outpust into any Machine learning component for instance: linear regression.

3. specify the working directore, for example "E:\ant\plugin\example**\**" (don't forget the backslash at the end of the directory path". 

4. add prediction data by using merge component, **(note: the total number of merged values must be equal to the number of features)**

5. start fitting your data ( use boolean toggle component) .. Thats. all.


![csvcomponent](https://user-images.githubusercontent.com/6969514/27114267-aac9b728-50c2-11e7-9d73-9cc7ca16eb19.jpg)

The same steps holds for Excel dataset:

![excel_dataset](https://user-images.githubusercontent.com/6969514/27114445-080d49b2-50c4-11e7-9c44-145f18129bde.jpg)


## Licence
Copyright (c) 2017, Mahmoud AbdelRahman <arch.mahmoud.ouf111@gmail.com> 

http://www.m-ouf.com/ 

All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.


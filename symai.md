## SymAI, a free, open source, symbolic regression library.

SymAI is in alpha, watch here for updates.

SymAI is written in C# and compiles with the free edition of Visual Studio 2019.

The SymAI library is a .NET library that you can integrate into your own projects. All of the important symbolic regression functions are in the Sym library. The project also includes a GUI, the SymRegressionApp.

SymAI was developed with typical regression problems in mind. SymAI can import a csv table containing multiple independent columns and a dependent column. It will use this data to form a model that can then predict forecasts given a 'row' of independent data. Unlike many other AI's SymAI can produce dense models that are represented as expressions. For example:

0.6069037145408354*(Independant[153]+Independant[176]*Independant[82])

The above example was created using settings that penalize long expressions. SymAI can produce more complex models but you can run into overfitting problems if you take that approach.

Clearly creating dense expressions instead of overly complex models has it's advantages. Speed of execution is one. Another is the ability to examine the list of expressions produced by hand to look for relationships in the data that SymAI has found.

SymAI can use your GPU to test thousands of models per second.

The SymAI was debugged using data from Kaggle's "House Prices: Advanced Regression Techniques". This help file will walk you through the process of using this data to train a regression based model, make forecasts, and submit your creation to Kaggle. Note that the solution to this contest is publicly available so the top of the leaderboard is full of unrealistic entries. Don't expect a winning entry to be produced by this alpha version, imrovements need to be made.

Let's begin.

Go to Kaggle and download and unzip the data file for "House Prices: Advanced Regression Techniques". Download and unzip the Sym repository. Load the Sym solution into Visual Studio and start the SymRegressionApp.

In the SymRegressionApp go to Windows, Process Data. Click the Import button and load the train.csv file that you downloaded from Kaggle. Click Find Categories and Expand categories. At this point it might be helpful to explain what you've done. Many .csv tables contain strings as well as numeric data. The core SymAI library can only operate upon numeric data so all of the strings have to be converted in a meaningful way into numbers. Find Categories finds the columns that contain a small set of different strings, and converts the strings to integers. Expand categories creates multiple new columns from these columns. The data is also normalized so that all values are between 0 and 1. Click the Process Data and Save button to save the new processed file and call it processedTrain.csv. Click the Process Definition button to save a file stating how the data was processed, call it processDefinition.pd. You'll need this process definition later when forecasting. Close the ProcessDataWindow.

Using the SymRegressionApp window, click File, Import and open the processedTrain.csv file that you created. Click Windows, Options and set the AcceleratorType to Cuda if you have a Nvidia GPU, OpenCL for AMD. Close the Options window. Click Start and wait a few moments. You should see a wall of text appear with the best models ranked towards the top. It'll take a few minutes for SymAI to produce a decent model. SymAI is built to keep on running and imroving it's models so if you want to give it a while that's fine too.

You can leave the AI training and produce a submission file for Kaggle. Under the Windows menu click Forecasting. Click the Load Data button. Load the test.csv file that you downloaded from Kaggle. Click the Load Process Definition button and load the processDefinition.pd file that you created earlier. Click Get Best Model to get the best model produced so far. Click Forecast and enter Id at the prompt. Change the name of the Forecast column to SalePrice. Click the Save Forecast button and save the file as submission.csv. Upload the submission.csv file to Kaggle using the process they describe to submit contest entries.

You ranking will likely not be spectacular. One, solutions to this public contest have been published so the leaderboard is full of unrealistic entries. Two, my app is alpha, improvements are coming. More processing power and time will likely help some. SymAI does produce very dense models that can be executed very quickly compared to other AI's. If this is a requirement of yours then you may find this approach appealing.

If you start playing with the library itself, note that the model forecasts normalized solutions between 0 and 1. These can be converted to proper forecasts with the ScaleAndOffsetForecast function in the Forecasting class. The scale and offset you'll need will be in any ProcessDefinition class you create. If you go through the SymRegressionApp code a bit you can get a top down view of using the library.










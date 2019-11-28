# ElasticSearchDemo
 Website with ElasticSearch (NEST) Implementation of Search Engine

# Prerequisites

## 64-bit Java

To begin, you'll need to have Java SE running on your machine. This is required for the installation of ElasticSearch, thus the step can be skipped if Java is already installed. I personally would advise installing the latest version [(13.0.1)](https://www.oracle.com/technetwork/java/javase/downloads/jdk13-downloads-5672538.html)

Once Java is installed, the following steps are required to set the proper Environment variables.

 - Windows search “advanced system settings”
 - Click “View advanced system settings”
 - In the “System Properties” window that opens up, click Environment Variables
 - Click on the “Path” system variable and click “edit”
 - Click “Browse”, and navigate to the “bin” folder of your installation directory.
 - Take the newly created entry and move it all the way to the top.

> **NOTE** [This](https://www3.ntu.edu.sg/home/ehchua/programming/howto/JDK_Howto.html) article can be used as a more thorough guide for setting Java up.

## ElasticSearch 6.8.5

You can install ElasticSearch [here](https://www.elastic.co/downloads/past-releases/elasticsearch-6-8-5). For the install process I recommend

 - Use default directories
 - Install as a service, using a Local System account, but not having the service run automatically on startup.
 - I name my Cluster "PrimaryCluster" and Node "CentralNode", leaving the other configuration settings as default.
 - I didn't install any extra plugins.
 - I used a basic license.
 - Run the service using the command 
 
 ```
 Invoke-RestMethod http://localhost:9200
 ```
 
 ## NET Core SDK
 
 Download the [.NET 3.0.101 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.0).
 
 # SampleDataConsumer (Optional)
 
 Within this code solution is a project I set up to seed the SQLite local database that's used as the primary data source for this demo. You do not need to run it, given that the website project included a seeded database file.
 
 The console application pulls from the TMDb API and fills up tables pertaining to movies and actors.
 
 # ElasticIndexer
 
 This console program MUST be run prior to running the website. It's fairly automated, and will index the data from the SQLite file for future usage. 
 
 There's no problem running this program multiple times, as it uses an alias system to keep a maximum of three indexes open at a time, with the referenced alias always pointing to the newest index.
 
 # ElasticSearchWebsite
 
 This is the website addressing the bulk of the given task. Of the advanced features provided, the site has built in
 
 - Stemming based upon English norms (example, Run => Run, Running, Runner)
 - Fuzzy searching, so slight typos still return valid results (I may tweak this in the future to be more strict than it currently is)
 - Auto-complete for text going into the query box
 - Genre meta-data for faceting searches
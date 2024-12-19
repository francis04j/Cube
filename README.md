# Transaction processor

[![Build and unit tests](https://github.com/francis04j/Cube/actions/workflows/build-and-unit-test.yml/badge.svg)](https://github.com/francis04j/Cube/actions/workflows/build-and-unit-test.yml)

Hello reviewer, thanks for taking your time to review this code. This code was completed over the weekend. I tried to time it at 4 hours per day.
The code isnt perfect but i hope it will show you how i structure my code and verify it. 
I'll appreciate any feedback

## How to use
This project uses .NET 8. Please make sure you have .NET 8 installed

To build the application, run 
$ dotnet build

To run it
$dotnet run

To run the unit tests
$cd CubeLogic.TransactionsConverter.UnitTests
dotnet test

## Docker
Containerizing the application allow us to achieve consistent deployments environements across dev, testing and production.
It also eliminate dependency issues because it packages all the required components into the container

To build and run this application using Docker, do the following
> $ docker build -t transconv . //transconv is the image name in this example
> $ docker run -u root -v $(pwd)/inputTransactions.csv:/app/inputTransactions.csv -v $(pwd)/config.json:/app/config.json -v $(pwd)/output:/app/output transconv

//the above command runs the docker image build in previous command
//it use the root user to run container via -u root, you can subsistute this for a user of your choice
//It maps and mounts 3 volumes
// First, -v $(pwd)/inputTransactions.csv:/app/inputTransactions.csv, for input transactions file
// Second, -v $(pwd)/config.json:/app/config.json
// Third, -v $(pwd)/output:/app/output

## Future updates
###1.Performance profiling
###2.Integration tests
###3.Deployment into Cloud (Azure)
###4. Improve test coverage
   
[![HitCount](https://hits.dwyl.com/francis04j/francis04j/Cube.svg?style=flat-square)](http://hits.dwyl.com/francis04j/francis04j/Cube)

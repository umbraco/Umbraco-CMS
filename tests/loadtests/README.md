# Load testing scripts for Umbraco

Will be used for running load tests against Umbraco for both back office (general coverage) and front-end (throughput).
The artillery scripts will be created in a way that will work between Umbraco versions so that we can collect usage reports
between versions.

To execute the test:

* Fill in the environment variable values in `run.ps1`
* then execute
    ```
    .\run.ps1
    ```

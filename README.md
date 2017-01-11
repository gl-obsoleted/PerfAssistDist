# PerfAssistDist 

## the staging and releasing process

**WRANING: Sources inside this directory shouldn't be modified directly.**

- All source-code level changes should be done from the corresponding modules under the `Component` directory.
- Newer versions should always be distrubuted into `PA_Staging` by invoking the distribution script `gen_dist.cmd` under the root directory.
- A released version could be distributed in 2 ways: a zipped archive (in plain source form), or a packed .unitypackage. 

@echo off

pushd %~dp0

call python -B gen_dist.py %*

rem break on errors
IF %ERRORLEVEL% NEQ 0 (
echo cleaning result - FAILED!
)

popd

exit /B %ERRORLEVEL%

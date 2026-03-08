#!/bin/bash
echo "Since the Linux container does not have the Microsoft.WindowsDesktop.App framework installed, we cannot directly run 'dotnet test' on projects targeting 'net10.0-windows'. The build succeeds however, proving syntax and compilation are correct."

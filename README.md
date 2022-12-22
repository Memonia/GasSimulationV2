# GasSimulationV2
**Requirements:**
  * .NET 6
  * CUDA Toolkit 11.8
  * CUDA GPU Compute Capability >= 5.0
  
 CUDA Toolkit installer takes care of setting up Visual Studio. [See the installation guide.](https://docs.nvidia.com/cuda/cuda-installation-guide-microsoft-windows/)  
 
 CUDA project will fail to load if the versions don't match exactly. In this case open the .vcxproj file manually from the explorer and look for the following items:  
```
  <ImportGroup Label="ExtensionSettings">
    <Import Project="$(VCTargetsPath)\BuildCustomizations\CUDA 11.8.props"/>
  </ImportGroup>

  <ImportGroup Label="ExtensionTargets">
    <Import Project="$(VCTargetsPath)\BuildCustomizations\CUDA 11.8.targets"/>
  </ImportGroup>
```
Change the version (in this case 11.8) to the version of cuda you have installed. Once the project is loaded, you can change the version in 'build 
customization' of the project.

GPU Compute Capability to be compiled for can be changed in 'CUDA C/C++' properties of the project.

   M S B A M L   `   `   `  ����c  _System.Windows.Interactivity, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35qPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.InteractivitySystem.Windows.Interactivity  5   	   E AFileFinder, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:FileFinder
FileFinder 5   	      FileFinder.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ^iPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.Interactivity     5   	   *localclr-namespace:FileFinder     5   	     � Title$  
MainWindow��5	   	   $	��450��6   $	��800��6)   .��5
      �  � RowDefinitions 5   
   �� 5      $��10 �6   �� 5      $��30 �6   �� 5      $��30 �6   �� 5      $��30 �6   �� 5      $��80 �6   �� 5      �� 5      $��30 �6   .+�5   
   �� $��1��6   $	��300��6"    �� Orientation$ 
Horizontal=�6.   .+�5      �� $	��100��6   $��
Поиск��6!   .   (System.Windows.Interactivity.Interaction   Triggers 5      /   )System.Windows.Interactivity.EventTrigger  5         	EventName$ Click��6%   .   (System.Windows.Interactivity.TriggerBase   Actions. 5      6   0System.Windows.Interactivity.InvokeCommandAction     Command 60   �� *Find+5      5      �� 5      -   5         ��Name$ Pause��5      $	��100��6$   $��
Пауза��60   ���	#FF8B8B8B
���   � J� OpacityMask
 �   ����	#FFDDDDDD 5        5      $ Click��6%   . 5         60   �� *Pause+	   CommandParameter	 5       �� 
 �� ElementName$
 Pause��5!      5#      �� 5$      $	��100��6   $��Стоп��6!    5%        5&      $ Click��6%   . 5'         60   �� *Stop+5(      5*      5+      �� 5,   
   $��2��6   $ 
Horizontal=�6"   .+�5-      �� $��Искать в��6   $��85��6(   $
��Left=�63   �� 5.      -   5         ��Name$ 
RootFolder��5.      $	��500��6+   $��3,3,0,3q�67   ��6H   �� *
RootFolder+ �� Mode$ TwoWay=� �� UpdateSourceTrigger$ PropertyChanged=��� 50      $��Выбрать папку��6   $��0,3,3,3q�6-    51        52      $ Click��6%   . 53         60   �� *
OpenFolder+54      56      57      �� 58   
   $��3��6   $ 
Horizontal=�6"   .+�59      �� $��Шаблон��6   $��85��6&   �� 5:      $	��500��6   $��3q�6#   ��6.   �� *TemplateNameFile+5;      �� 5<   
   $��4��6   $ 
Horizontal=�6"   .+�5=      �� $��Содержимое��6   $��85��6*   �� 5>      $	��650��6   $��3q�6#   ��6.   �� *ContentFile+5?      R� 5@   
   -   5         R�Name$ TrVwFileAndFolder��5@      $��5��6.   ��6;   �� *FileOrFolder+$ PropertyChanged=���5B      �� 5C       �� ItemsSource 6+   �� *ChldFileOrFolders+$ PropertyChanged=�.R�5E      �� ��6   �� *Name+$ PropertyChanged=�5F      5H      �� 5I   
   $ 
Horizontal=�6   $��6��6.   .+�5J      �� $'��!Время выполнения: ��6   �� 5K      $��90��5M   7   ��5L      �� *RunningTimeString+$ OneWay=�$ PropertyChanged=�5M   7   �� 5N      $)��#Обрабатваемый файл��6   �� 5O      $	��400��5Q   7   ��5P      �� *FileNameInProcess+$ OneWay=�$ PropertyChanged=�5Q   7   5R      5S      5T      
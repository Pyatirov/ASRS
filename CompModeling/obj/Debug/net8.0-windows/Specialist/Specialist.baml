   M S B A M L   `   `   `  ����G  CCompModeling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:CompModelingCompModeling  5   	   R Nnetstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd514)clr-namespace:System;assembly=netstandardSystem 62   &     CompModeling.SpecialistInterface   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ,localclr-namespace:CompModeling     5   	   9sys)clr-namespace:System;assembly=netstandard     62     � Title$  Специалист��5   	   $	��200��6   $	��350��6)    � 
ResizeMode$ NoResize=�65   .��5	      �  � ColumnDefinitions 5
   
   �� 5       � RowDefinitions 5   
   �� 5      $
��0.3* �6   �� 5      $
��0.3* �6   �� 5      $
��0.3* �6   �� 5      $
��0.3* �6   .+�5   
   �� $
��Left=�6   $
��Wrap=�6/   $ ��Метод решения:��6C   $��Center=�6Y   $��10,0,0,0q�6t   �� 5   
   $
��Left=�6   $
��Wrap=�6/   $&�� Точность решения:��6C   $��Center=�6\   $��1��6w   $��10,0,0,0q�6�   �� 5   
   $
��Left=�6   $��2��6/   $
��Wrap=�6<   $,��&Макс. число итераций:��6P   $��Center=�6m   $��39��6�   $	��111��6�   $��10,0,0,0q�6�   �� 5   
   -   5         ��Name$ cb_SolutionMethod��5       �� DisplayMemberPath$ .��6.    �� Text$. (Метод Ньютона-Рафсона��6D   $
��Left=�6a   $��	129,0,0,0q�6|   $��Center=�6�   $	��195��6�   .@�5      ��,(Метод Ньютона-Рафсона��60   5      �� 5   
   -   5         ��Name$ tb_SolutionPrecision��5      $
��Left=�60   $��1��6K    �� TextWrapping$
 Wrap=�6X   $�� ��6l   $��Center=�6t   $	��120��6�   $��	129,0,0,0q�6�   �� 5   
   -   5        $ tb_MaxCountOfIterations��5      $
��Left=�63   $��2��6N   $
 Wrap=�6[   $�� ��6o   $��Center=�6w   $	��120��6�   $��	129,0,0,0q�6�   �� 5   
   $��Сохранить��6   $��Center=�6%   $��3��6B   $��Center=�6O   $��26��6j   $��81��6v   5      5      
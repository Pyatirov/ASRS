   M S B A M L   `   `   `  ����G  CCompModeling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:CompModelingCompModeling  5   	   &     CompModeling.ResearcherInterface   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ,localclr-namespace:CompModeling     5   	   -   5          � Title$   Исследователь��5   	   $	��700��6   $	��450��6,    � 
ResizeMode$ NoResize=�68    � 	Resources 5	   
   +   %CompModeling.RowToVisibilityConverter   RowToVisibilityConverter��%   &	          5
      6-   5      .��5      � .+�5   
   �� .@�5      �� $��Эксперимент��6   .��5      �  � RowDefinitions 5      �� 5      $
��Auto �6(   �� 5      $
��Auto �6(   �� 5      $��* �6(   �� 5      $
��Auto �6(   �� 5      $
��Auto �6(   .+�5      �� $��0��6    $!��Модель реакции��6-   $
��10,5q�6C   �� 5      -   5         ��Name$ cb_Mechanisms_Experiment��5      $��1��6A   $��10,0, 0, 10q�6N   $	��252��6c   $��25��6o    �� DisplayMemberPath$
 Info��6{   $
��Left=�6�   6�   �� 5      $��2��6#   $
��Auto=�60   $��Disabled=�6S   .��5      A� -   5         A�Name$ ug_Constants_Inputs_Panel��5   &    A� Columns$ 1��6I   $��10q�6U   $	��Top=�6a   $��Center=�6y   5      �� 5      -   5        $��3��5   8   $��Расчет��6E   $��10,10q�6V   $��40��6e   $	��200��6q   5!      5"      �� 5#      $!��Модели реакции��6   .��5$      �  5%      �� 5&      $��* �6(   �� 5'      $
��Auto �6(   .+�5*      &   System.Windows.Controls.DataGrid  -   5          Name$ dg_Mechanisms��5*      	   AutoGenerateColumns$	 False��66   $��
10,10,10,0q�6R   $��0��6f   $��2��6s   "
   VerticalScrollBarVisibility$
 Visible=�6�      Columns 5+      0  *System.Windows.Controls.DataGridTextColumn  5,      ,  &System.Windows.Controls.DataGridColumn   Header$ ID��61      Width5  /System.Windows.Controls.DataGridLengthConverter$ 30 6T   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6=   �� *ID+6T     5-      $ Описание��61   $ * 6\    6C   �� *Info+6\   4  .System.Windows.Controls.DataGridTemplateColumn  5.      $
 Auto 65      CellTemplate 5/   "   �� 50   &   .R�51   *   �� -   5        $��Удалить��51   1   $	��5 2q�6C   $��3q�6Q   
 � Tag 6~   �� *ID+y�6�   ��  �� 	Converter# [  52   '   54      56      �� 57      -   5        $#��Добавить модель��57      $��10,10,10,10q�6V   $��1��6k   $��2��6x   $��Center=�6�   $��40��6�   $	��200��6�   $	��Top=�6�   58      59      �� 5:      $3��-Экспериментальные точки��6   .��5;      �  5<      �� 5=      $
��Auto �6(   �� 5>      $
��Auto �6(   �� 5?      $��* �6(   �� 5@      $
��Auto �6(   .+�5C      �� $��0��6    $3��-Выберите модель реакции:��6-   $
��10,5q�6M   �� 5D      -   5        $ cb_Mechanisms_Points��5D      $��1��6=   $��	10,0,0,10q�6J   $��Center=�6]   $
 Info��6x   $
��Left=�6�   $��25��6�   $	��252��6�   �� 5F      $��2��6#   $
��Auto=�60   $��Disabled=�6S   .��5G      �� -	   5         ��Name$ pointInputsPanel��5G   %   $��10q�6?   $	��Top=�6K   $��Center=�6c   5H      �� 5J      -
   5        $��3��5J      $!��Добавить точку��6*   $��10,10q�6C   $��40��6R   $	��200��6^   6j   5K      5L      5M      5N      5O      
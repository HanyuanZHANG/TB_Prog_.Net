   M S B A M L   `   `   `  ����H  DThreadControl, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:ThreadControlThreadControl  5   	       ThreadControl.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:ThreadControl     5   	     � Title$  Gestion de Threads��5   	   ���	#FFB6BCC5$	��350��6;   $	��525��6H   .��5	      � .+�5
   
   o� -   5         o�Name$
 menu��5
      $
��Left=�6   ���	#FFB6BCC5$��43��6O   $	��Top=�6[   $	��520��6s   .@�5      m� $��Lancement threads��6   $��43��62   $	��140��6>   .@�5      m� -   5        $��Thread Ballon��5      63   �� 5      m� 5      -   5        $��Thread Premier��5      63   5      m� 5      -   5        $��Pause/Relance��5      $��43��6D   $	��120��6P   m� 5      $��Suppression��6   $��43��6,   $	��120��68   .@�5      m� -   5        $$��supprimer dernier thread crée��5      6B   �� 5      m� 5      -   5        $D��>supprimer dernier thread crée de la classe des threads Ballon��5      6b   �� 5      m� 5      -   5        $E��?supprimer dernier thread crée de la classe des threads Premier��5      6c   �� 5      m� 5      -   5        $'��!supprimer tout les threads crées��5      6E   5      m� 5      -	   5        $��Quitter��5      $��43��6;   $	��120��6G   5      �� 5   
   -
   5         ��Name$ listView��5      $
��Left=�6%   $	��234��6@   $��0,48,0,0q�6M   $	��Top=�6_   $	��520��6w   ��6�   �� *thread_list+�� 5   
   -   5         ��Name$ label��5      $��Nombre Ballon: ��6   $
��Left=�69   $��
10,288,0,0q�6T   $	��Top=�6h   �� 5   
   -   5         ��Name$ TextNbBallon��5      ���	#FFB6BCC5$
��Left=�6?   $��20��6Z   $��113,292,0,0q�6f    �� TextWrapping$
 Wrap=�6{   $��0��6�   $	��Top=�6�   $��40��6�   �� 5   
   -   5        $ label1��5      $��Nombre Premier:��6    $
��Left=�6:   $��206,288,0,0q�6U   $	��Top=�6j   �� 5    
   -   5        $ TextNbPremier��5       ���	#FFB6BCC5$
��Left=�6@   $��20��6[   $��317,292,0,0q�6g   $
 Wrap=�6|   $��0��6�   $	��Top=�6�   $��40��6�   5!      5"      
   M S B A M L   `   `   `  ����   Vizzit Tools    Vizzit_Tools.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$  Vizzit Tools��$	��800��$
��1200��.��� $��0,0,0,3q�.+��� -    ��Name$ DomainTextBox��$
��Left=�$��23��$��	80,30,0,0q� �� TextWrapping$
 Wrap=�$	��Top=�$	��197��$��1���� -    ��Name$ DomainTextBlock��$
��Left=�$��	10,30,0,0q�$
��Wrap=�$��Domain��$	��Top=��� -    ��Name$ StartButton��$��Crawl��$
��Left=�$��
603,58,0,0q�$	��Top=�$��75�� J� RenderTransformOrigin$ -0.08,-0.15&�$��23���� -   $ DebugTextBlock��$
��Left=�$��986,152,0,-414q�$
��Wrap=�$	��Top=�$
��1028��$	��196��$��Visible=���. �� -   $ StartPageTextBox��$
��Left=�$��23��$��	80,59,0,0q�$
 Wrap=�$	��Top=�$	��197��$��2���� -   $ StartPageTextBlock��$
��Left=�$��	10,58,0,0q�$
��Wrap=�$��	Startpage��$	��Top=��� -   $ AddBtn��$	��Add��$
��Left=�$��
282,87,0,0q�$	��Top=�$��75��$��22���� -   $ 	RemoveBtn��$��Remove��$
��Left=�$��404,659,0,0q�$	��Top=�$��75��$ 	0.92,-0.2&�$��24��	� -	    	�Name$ ListRadioBtn��$
��List��$
��Left=�$��	6,152,0,0q�$	��Top=�	� -
   $ 
DbRadioBtn��$��DB��$
��Left=�$��
48,152,0,0q�$	��Top=�$ -0.012,0.667&��� -    ��Name$ CustomerLsv��$
��Left=�$	��479��$��	6,175,0,0q�$	��Top=�$	��472�� �� View �� .P��� $��Domain��	 �� Width$		 230��
 �� DisplayMemberBinding
 �� *Domain+�� $��	Startpage��$		 120��
 �� *	Startpage+�� $��Customer ID��$		 120��
 �� *Id+�� -   $ CustomerIdTextBox��$
��Left=�$��23��$��	80,87,0,0q�$
 Wrap=�$	��Top=�$	��197��$��3���� -   $ CustIdTextBlock��$
��Left=�$��	10,87,0,0q�$
��Wrap=�$��Customer Id��$	��Top=��� -   $ ClrBtn��$��Clear��$
��Left=�$��321,659,0,0q�$	��Top=�$��78��$��24���� -   $ threadTextBlock��$
��Left=�$��
490,30,0,0q�$
��Wrap=�$��Threads��$	��Top=��� -   $ coreTextBox��$
��Left=�$��23��$��
558,30,0,0q�$
 Wrap=�$	��Top=�$	��120��$ -0.57,-0.107&�$��4���� -   $ cancelButton��$
��Stop��$
��Left=�$��
603,86,0,0q�$	��Top=�$��75��$��23���� -    ��Name$ dailyCheckBox��$��DailyCheckBox��$
��Left=�$��121,152,0,0q�$	��Top=�$ 	0.857,1.2&��� -   $ SendFileCheckBox��$��Sendfile��$
��Left=�$��235,152,0,0q�$	��Top=� d� 	IsChecked$
 TrueY��� -   $ quitBtn��$
��Quit��$
��Left=�$��603,114,0,0q�$	��Top=�$��75��$��23���� -   $ RunningCustomersLV��$��490,175,202,0q�$	��500�� �� .P��� $��Customer��$		 325��
 �� *
Customer+�� $
��Time��$		 175��
 �� *Time+
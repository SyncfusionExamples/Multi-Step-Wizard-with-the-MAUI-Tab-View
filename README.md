# Multi-Step-Wizard-with-the-MAUI-Tab-View

This sample demonstrates how to create a multi-step wizard using the .NET MAUI Tab View control in a .NET MAUI application.

## Sample

```xaml
     <Grid RowDefinitions="Auto,*" RowSpacing="12">
    <!-- Header -->
    <VerticalStackLayout Spacing="4">
        <Label Text="Event Registration Wizard"
               FontSize="22"
               TextColor="{StaticResource PrimaryColor}"
               FontAttributes="Bold"/>
    </VerticalStackLayout>

    <!-- Tabs -->
    <tabView:SfTabView x:Name="TabView" Grid.Row="1" EnableSwiping="False" BackgroundColor="Transparent"
                       TabWidthMode="{OnPlatform Android=SizeToContent, iOS=SizeToContent}">
        <tabView:SfTabView.TabBarBackground>
            <LinearGradientBrush EndPoint="0,1">
                <GradientStop Color="LightBlue" Offset="0.1" />
                <GradientStop Color="CornflowerBlue" Offset="1.0" />
        </LinearGradientBrush>
    </tabView:SfTabView.TabBarBackground>

        <!-- TAB 1: Personal Info -->
        <tabView:SfTabItem x:Name="TabPersonal" Header="1. Personal Info" IsVisible="True"
                           FontAttributes="Bold" FontSize="18">
            <tabView:SfTabItem.Content>
                <local:PersonalInfoView x:Name="PersonalView" />
            </tabView:SfTabItem.Content>
        </tabView:SfTabItem>

        <!-- TAB 2: Event Selection -->
        <tabView:SfTabItem x:Name="TabEvent" Header="2. Event Selection" IsVisible="False"
                           FontAttributes="Bold" FontSize="18">
            <tabView:SfTabItem.Content>
                <local:EventSelectionView x:Name="EventView" />
            </tabView:SfTabItem.Content>
        </tabView:SfTabItem>

        <!-- TAB 3: Accommodation Preferences -->
        <tabView:SfTabItem x:Name="TabAccommodation" Header="3. Accommodation" IsVisible="False"
                           FontAttributes="Bold" FontSize="18">
            <tabView:SfTabItem.Content>
                <local:AccommodationView x:Name="Accommodation" />
            </tabView:SfTabItem.Content>
        </tabView:SfTabItem>

        <!-- TAB 4: Payment Details -->
        <tabView:SfTabItem x:Name="TabPayment" Header="4. Payment" IsVisible="False"
                           FontAttributes="Bold" FontSize="18">
            <tabView:SfTabItem.Content>
                <local:PaymentView x:Name="Payment" />
            </tabView:SfTabItem.Content>
        </tabView:SfTabItem>

    </tabView:SfTabView>
</Grid>
```

### Output

![Tab View](FormSample2025-12-0109-50-46-ezgif.com-video-to-gif-converter.gif)

## Requirements to run the demo

To run the demo, refer to [System Requirements for .NET MAUI](https://help.syncfusion.com/maui/system-requirements)

## Troubleshooting:

### Path too long exception

If you are facing path too long exception when building this example project, close Visual Studio and rename the repository to short and build the project.

## License

Syncfusion has no liability for any damage or consequence that may arise from using or viewing the samples. The samples are for demonstrative purposes. If you choose to use or access the samples, you agree to not hold Syncfusion liable, in any form, for any damage related to use, for accessing, or viewing the samples. By accessing, viewing, or seeing the samples, you acknowledge and agree Syncfusion's samples will not allow you seek injunctive relief in any form for any claim related to the sample. If you do not agree to this, do not view, access, utilize, or otherwise do anything with Syncfusion's samples.

### Instalando

Nuget: https://www.nuget.org/packages/WaxComponents

- Adicionar na head do html
```html
<!-- Theme first -->
<link href="_content/WaxComponents/Styles/Themes/lightPink.css" rel="stylesheet">

<!-- Styles -->
<link href="_content/WaxComponents/Styles/waxComponents.css" rel="stylesheet">

<!-- Fonts -->
<link rel="preconnect" href="https://fonts.googleapis.com">
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
<link href="https://fonts.googleapis.com/css2?family=JetBrains+Mono&display=swap" rel="stylesheet">
<link href="https://fonts.googleapis.com/icon?family=Material+Icons" rel="stylesheet">
```

- Importar a biblioteca
```cs
//...
@using WaxComponents
```

<br/>

### Usando

```html
<!-- Button -->
<WaxButton OnClick="OnClick">
    
    <!--Icones baseados no tema Material-->
    <!--https://fonts.google.com/icons-->
    <Icon><WaxIcon Size="24px">done</WaxIcon></Icon>
    <Text>Ceira</Text>
</WaxButton>

<!-- Text Input -->
<WaxTextInput @bind-Value="_text" Placeholder="Ceira"></WaxTextInput>
```

```c#
private string _text = String.Empty;

private void OnClick(object? sender, EventArgs e)
{
    Console.WriteLine(_text);
}
```

<br/>

### Resultado
![sample](https://raw.githubusercontent.com/lllggghhhaaa/WaxComponents/master/Assets/sample.png)
![sample](https://raw.githubusercontent.com/lllggghhhaaa/WaxComponents/master/Assets/sample.gif)

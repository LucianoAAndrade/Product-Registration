function set_dados_form(dados) {
    $('#id_cadastro').val(dados.Id);
    $('#txt_nome').val(dados.Name);
    $('#txt_email').val(dados.Email);
    $('#txt_login').val(dados.Login);
    $('#txt_Key').val(dados.Key);
}

function set_focus_form() {
    $('#txt_nome').focus();
}

function get_dados_inclusao() {
    return {
        Id: 0,
        Name: '',
        Email: '',
        Login: '',
        Key: ''
    };
}

function get_dados_form() {
    return {
        Id: $('#id_cadastro').val(),
        Name: $('#txt_nome').val(),
        Email: $('#txt_email').val(),
        Login: $('#txt_login').val(),
        Key: $('#txt_Key').val()
    };
}

function preencher_linha_grid(param, linha) {
    linha
        .eq(0).html(param.Name).end()
        .eq(1).html(param.Login);
}

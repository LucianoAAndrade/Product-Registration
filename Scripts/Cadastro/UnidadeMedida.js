function set_dados_form(dados) {
    $('#id_cadastro').val(dados.Id);
    $('#txt_nome').val(dados.Name);
    $('#txt_sigla').val(dados.Initials);
    $('#cbx_ativo').prop('checked', dados.Active);
}

function set_focus_form() {
    $('#txt_nome').focus();
}

function set_dados_grid(dados) {
    return '<td>' + dados.Name + '</td>' +
           '<td>' + dados.Initials + '</td>' +
           '<td>' + (dados.Active ? 'SIM' : 'NÃO') + '</td>';
}

function get_dados_inclusao() {
    return {
        Id: 0,
        Name: '',
        Initials: '',
        Active: true
    };
}

function get_dados_form() {
    return {
        Id: $('#id_cadastro').val(),
        Name: $('#txt_nome').val(),
        Initials: $('#txt_sigla').val(),
        Active: $('#cbx_ativo').prop('checked')
    };
}

function preencher_linha_grid(param, linha) {
    linha
        .eq(0).html(param.Name).end()
        .eq(1).html(param.Initials).end()
        .eq(2).html(param.Active ? 'SIM' : 'NÃO');
}

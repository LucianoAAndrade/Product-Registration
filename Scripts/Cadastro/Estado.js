function set_dados_form(dados) {
    $('#id_cadastro').val(dados.Id);
    $('#txt_nome').val(dados.Name);
    $('#txt_uf').val(dados.UF);
    $('#ddl_pais').val(dados.IdCountries);
    $('#cbx_ativo').prop('checked', dados.Active);
}

function set_focus_form() {
    $('#txt_nome').focus();
}

function get_dados_inclusao() {
    return {
        Id: 0,
        Name: '',
        UF: '',
        IdCountries: 0,
        Active: true
    };
}

function get_dados_form() {
    return {
        Id: $('#id_cadastro').val(),
        Name: $('#txt_nome').val(),
        UF: $('#txt_uf').val(),
        IdCountries: $('#ddl_pais').val(),
        Active: $('#cbx_ativo').prop('checked')
    };
}

function preencher_linha_grid(param, linha) {
    linha
        .eq(0).html(param.Name).end()
        .eq(1).html(param.UF).end()
        .eq(2).html(param.Active ? 'SIM' : 'NÃO');
}

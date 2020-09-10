function set_dados_form(dados) {
    $('#id_cadastro').val(dados.Id);
    $('#txt_nome').val(dados.Name);
    $('#ddl_pais').val(dados.IdCountries);
    $('#cbx_ativo').prop('checked', dados.Active);

    $('#ddl_estado').val(dados.IdStates);
    $('#ddl_estado').prop('disabled', dados.IdStates <= 0 || dados.IdStates == undefined);
}

function set_focus_form() {
    $('#txt_nome').focus();
}

function get_dados_inclusao() {
    return {
        Id: 0,
        Name: '',
        IdCountries: 0,
        IdStates: 0,
        Active: true
    };
}

function get_dados_form() {
    return {
        Id: $('#id_cadastro').val(),
        Name: $('#txt_nome').val(),
        IdCountries: $('#ddl_pais').val(),
        IdStates: $('#ddl_estado').val(),
        Active: $('#cbx_ativo').prop('checked')
    };
}

function preencher_linha_grid(param, linha) {
    linha
        .eq(0).html(param.Name).end()
        .eq(1).html(param.Active ? 'SIM' : 'NÃO');
}

$(document).on('change', '#ddl_pais', function () {
    var ddl_pais = $(this),
        id_pais = parseInt(ddl_pais.val()),
        ddl_estado = $('#ddl_estado');

    if (id_pais > 0) {
        var url = url_listar_estados,
            param = { idCountries: id_pais };

        ddl_estado.empty();
        ddl_estado.prop('disabled', true);

        $.post(url, add_anti_forgery_token(param), function (response) {
            if (response && response.length > 0) {
                for (var i = 0; i < response.length; i++) {
                    ddl_estado.append('<option value=' + response[i].Id + '>' + response[i].Name + '</option>');
                }
                ddl_estado.prop('disabled', false);
            }
        });
    }
});
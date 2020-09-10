function set_dados_form(dados) {
    $('#id_cadastro').val(dados.Id);
    $('#txt_nome').val(dados.Name);
    $('#txt_num_documento').val(dados.NumDocumento);
    $('#txt_razao_social').val(dados.RazaoSocial);
    $('#txt_telefone').val(dados.Telefone);
    $('#txt_contato').val(dados.Contato);
    $('#txt_logradouro').val(dados.Logradouro);
    $('#txt_complemento').val(dados.Complemento);
    $('#txt_cep').val(dados.Cep);
    $('#ddl_pais').val(dados.IdCountries);
    $('#ddl_estado').val(dados.IdStates);
    $('#ddl_cidade').val(dados.IdCity);
    $('#cbx_ativo').prop('checked', dados.Active);
    $('#cbx_pessoa_juridica').prop('checked', false);
    $('#cbx_pessoa_fisica').prop('checked', false);

    if (dados.Tipo == 2) {
        $('#cbx_pessoa_juridica').prop('checked', true).trigger('click');
    }
    else {
        $('#cbx_pessoa_fisica').prop('checked', true).trigger('click');
    }

    $('#ddl_estado').prop('disabled', dados.IdStates <= 0 || dados.IdStates == undefined);
    $('#ddl_cidade').prop('disabled', dados.IdCity <= 0 || dados.IdCity == undefined);
}

function set_focus_form() {
    $('#txt_nome').focus();
}

function get_dados_inclusao() {
    return {
        Id: 0,
        Name: '',
        NumDocumento: '',
        RazaoSocial: '',
        Tipo: 2,
        Telefone: '',
        Contato: '',
        Logradouro: '',
        Complemento: '',
        Cep: '',
        IdCountries: 0,
        IdStates: 0,
        IdCity: 0,
        Active: true
    };
}

function get_dados_form() {
    return {
        Id: $('#id_cadastro').val(),
        Name: $('#txt_nome').val(),
        NumDocumento: $('#txt_num_documento').val(),
        RazaoSocial: $('#txt_razao_social').val(),
        Tipo: $('#cbx_pessoa_juridica').is(':checked') ? 2 : 1,
        Telefone: $('#txt_telefone').val(),
        Contato: $('#txt_contato').val(),
        Logradouro: $('#txt_logradouro').val(),
        Complemento: $('#txt_complemento').val(),
        Cep: $('#txt_cep').val(),
        IdCountries: $('#ddl_pais').val(),
        IdStates: $('#ddl_estado').val(),
        IdCity: $('#ddl_cidade').val(),
        Active: $('#cbx_ativo').prop('checked')
    };
}

function preencher_linha_grid(param, linha) {
    linha
        .eq(0).html(param.Name).end()
        .eq(1).html(param.Telefone).end()
        .eq(1).html(param.Contato).end()
        .eq(2).html(param.Active ? 'SIM' : 'NÃO');
}

$(document)
.ready(function () {
    $('#txt_telefone').mask('(00) 0000-0000');
    $('#txt_cep').mask('00000-000');
})
.on('click', '#cbx_pessoa_juridica', function () {
    $('label[for="txt_num_documento"]').text('CNPJ');
    $('#txt_num_documento').mask('00.000.000/0000-00', { reverse: true });
    $('#container_razao_social').removeClass('invisible');
})
.on('click', '#cbx_pessoa_fisica', function () {
    $('label[for="txt_num_documento"]').text('CPF');
    $('#txt_num_documento').mask('000.000.000-00', { reverse: true });
    $('#container_razao_social').addClass('invisible');
})
.on('change', '#ddl_pais', function () {
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
})
.on('change', '#ddl_estado', function () {
    var ddl_estado = $(this),
        id_estado = parseInt(ddl_estado.val()),
        ddl_cidade = $('#ddl_cidade');

    if (id_estado > 0) {
        var url = url_listar_cidades,
            param = { idStates: id_estado };

        ddl_cidade.empty();
        ddl_cidade.prop('disabled', true);

        $.post(url, add_anti_forgery_token(param), function (response) {
            if (response && response.length > 0) {
                for (var i = 0; i < response.length; i++) {
                    ddl_cidade.append('<option value=' + response[i].Id + '>' + response[i].Name + '</option>');
                }
                ddl_cidade.prop('disabled', false);
            }
        });
    }
});
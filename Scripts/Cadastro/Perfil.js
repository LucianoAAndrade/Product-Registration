function set_dados_form(dados) {
    $('#id_cadastro').val(dados.Id);
    $('#txt_nome').val(dados.Name);
    $('#cbx_ativo').prop('checked', dados.Active);

    var lista_usuario = $('#lista_usuario');
    lista_usuario.find('input[type=checkbox]').prop('checked', false);

    if (dados.User) {
        for (var i = 0; i < dados.User.length; i++) {
            var usuario = dados.User[i];
            var cbx = lista_usuario.find('input[data-id-usuario=' + usuario.Id + ']');
            if (cbx) {
                cbx.prop('checked', true);
            }
        }
    }
}

function set_focus_form() {
    $('#txt_nome').focus();
}

function get_dados_inclusao() {
    return {
        Id: 0,
        Name: '',
        Active: true
    };
}

function get_dados_form() {
    return {
        Id: $('#id_cadastro').val(),
        Name: $('#txt_nome').val(),
        Active: $('#cbx_ativo').prop('checked'),
        idUser: get_lista_usuarios_marcados()
    };
}

function preencher_linha_grid(param, linha) {
    linha
        .eq(0).html(param.Name).end()
        .eq(1).html(param.Active ? 'SIM' : 'NÃO');
}

function get_lista_usuarios_marcados() {
    var ids = [],
        lista_usuario = $('#lista_usuario');

    lista_usuario.find('input[type=checkbox]').each(function (index, input) {
        var cbx = $(input),
            marcado = cbx.is(':checked');

        if (marcado) {
            ids.push(parseInt(cbx.attr('data-id-usuario')));
        }
    });

    return ids;
}
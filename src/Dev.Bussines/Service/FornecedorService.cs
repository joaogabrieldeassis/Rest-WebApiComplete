﻿using Dev.Bussines.Interfaces;
using Dev.Bussines.Model.Validation;
using MinhaAp.Busines.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Bussines.Service
{
    public class FornecedorService : BaseService, IFornecedorService
    {
        #region Private Fields

        private readonly IFornecedorRepository _fornecedorRepository;
        private readonly IEnderecoRepository _enderecoRepository;

        #endregion Private Fields

        #region Public Constructors

        public FornecedorService(IFornecedorRepository fornecedorRepository,
                                 IEnderecoRepository enderecoRepository,
                                 INotificador notificador) : base(notificador)
        {
            _fornecedorRepository = fornecedorRepository;
            _enderecoRepository = enderecoRepository;
        }

        #endregion Public Constructors

        #region Public Methods

        public async Task<bool> Adicionar(Fornecedor fornecedor)
        {
            if (!ExecutarValidacao(validacao: new FornecedorValidation(),
                                   entidade: fornecedor)
                || !ExecutarValidacao(validacao: new EnderecoValidation(),
                                      entidade: fornecedor.Endereco))
                return false;

            if (_fornecedorRepository.Buscar(_fornecedor => _fornecedor.Documento.Equals(fornecedor.Documento)).Result
                                     .Any())
            {
                Notificar("Já existe um fornecedor com este documento informado.");
                return false;
            }

            await _fornecedorRepository.Adicionar(fornecedor);
            return true;
        }

        public async Task<bool> Atualizar(Fornecedor fornecedor)
        {
            if (!ExecutarValidacao(validacao: new FornecedorValidation(),
                                   entidade: fornecedor))
                return false;

            if (_fornecedorRepository.Buscar(_fornecedor => _fornecedor.Documento.Equals(fornecedor.Documento)
                                                         && _fornecedor.Id != fornecedor.Id).Result
                                     .Any())
            {
                Notificar("Já existe um fornecedor com este documento informado.");
                return false;
            }

            await _fornecedorRepository.Atualizar(fornecedor);
            return true;
        }        

        public async Task<bool> Remover(Guid id)
        {
            if (_fornecedorRepository.ObterFornecedorProdutosEndereco(id).Result.Produtos
                                     .Any())
            {
                Notificar("O fornecedor possui produtos cadastrados!");
                return false;
            }

            Endereco _endereco = await _enderecoRepository.ObterEnderecoPorFornecedor(id);

            if (_endereco != null)
                await _enderecoRepository.Deletar(_endereco.Id);

            await _fornecedorRepository.Deletar(id);
            return true;
        }
        public async Task AtualizarEndereco(Endereco endereco)
        {
            if (!ExecutarValidacao(validacao: new EnderecoValidation(),
                                   entidade: endereco))
                return;

            await _enderecoRepository.Atualizar(endereco);
        }
        public void Dispose()
        {
            _fornecedorRepository?.Dispose();
            _enderecoRepository?.Dispose();
        }

        #endregion Public Methods
    }
}
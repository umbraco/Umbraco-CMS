
import {DocumentDomainBuilder} from './documentDomainBuilder';
import {DocumentDomainValue} from '../../types';

export class DocumentDomainValueBuilder {
  parentBuilder: DocumentDomainBuilder;
  domainName: string;
  isoCode: string;

  constructor(parentBuilder: DocumentDomainBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withDomainName(domainName: string) {
    this.domainName = domainName;
    return this;
  }

  withIsoCode(isoCode: string) {
    this.isoCode = isoCode;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  build(): DocumentDomainValue {
    return {
      domainName: this.domainName,
      isoCode: this.isoCode,
    };
  }
}
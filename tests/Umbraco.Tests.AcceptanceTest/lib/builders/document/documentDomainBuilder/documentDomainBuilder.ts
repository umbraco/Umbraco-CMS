import {DocumentDomainValueBuilder} from './documentDomainValueBuilder';
import {DocumentDomainsPayload} from '../../types';

export class DocumentDomainBuilder {
  defaultIsoCode: string;
  documentDomainValueBuilder: DocumentDomainValueBuilder[];

  constructor() {
    this.documentDomainValueBuilder = [];
  }

  withDefaultIsoCode(defaultIsoCode: string) {
    this.defaultIsoCode = defaultIsoCode;
    return this;
  }

  addDomain() {
    const builder = new DocumentDomainValueBuilder(this);
    this.documentDomainValueBuilder.push(builder);
    return builder;
  }

  build(): DocumentDomainsPayload {
    return {
      domains: this.documentDomainValueBuilder.map(builder => builder.build()),
      defaultIsoCode: this.defaultIsoCode || null,
    };
  }
}
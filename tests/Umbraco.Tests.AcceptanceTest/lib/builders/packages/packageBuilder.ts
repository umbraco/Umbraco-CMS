export class PackageBuilder {
  name: string;
  contentNodeId: string;
  contentLoadChildNodes: boolean;
  mediaIds: string[];
  mediaLoadChildNodes: boolean;
  documentTypes: string[];
  mediaTypes: string[];
  dataTypes: string[];
  templates: string[];
  partialViews: string[]
  stylesheets: string[]
  scripts: string[];
  languages: string[]
  dictionaryItems: string[];

  withName(name: string) {
    this.name = name;
    return this;
  }

  withContentNodeId(contentNodeId: string) {
    this.contentNodeId = contentNodeId;
    return this;
  }

  withLoadContentChildNodes(contentLoadChildNodes: boolean) {
    this.contentLoadChildNodes = contentLoadChildNodes;
    return this;
  }

  withMediaIds(mediaIds: string[]) {
    this.mediaIds = mediaIds;
    return this;
  }

  withLoadMediaChildNodes(mediaLoadChildNodes: boolean) {
    this.mediaLoadChildNodes = mediaLoadChildNodes;
    return this;
  }

  withDocumentTypes(documentTypes: string[]) {
    this.documentTypes = documentTypes;
    return this;
  }

  withMediaTypes(mediaTypes: string[]) {
    this.mediaTypes = mediaTypes;
    return this;
  }

  withDataTypes(dataTypes: string[]) {
    this.dataTypes = dataTypes
    return this;
  }

  withTemplates(templates: string[]) {
    this.templates = templates;
    return this;
  }

  withPartialViews(partialViews: string[]) {
    this.partialViews = partialViews;
    return this;
  }

  withStylesheets(stylesheets: string[]) {
    this.stylesheets = stylesheets;
    return this;
  }

  withScripts(scripts: string[]) {
    this.scripts = scripts;
    return this;
  }

  withLanguages(languages: string[]) {
    this.languages = languages;
    return this;
  }

  withDictionaryItems(dictionaryItems: string[]) {
    this.dictionaryItems = dictionaryItems;
    return this;
  }

  build() {
    return {
      name: this.name,
      contentNodeId: this.contentNodeId || '',
      contentLoadChildNodes: this.contentLoadChildNodes !== undefined ? this.contentLoadChildNodes : true,
      mediaIds: this.mediaIds,
      mediaLoadChildNodes: this.mediaLoadChildNodes !== undefined ? this.mediaLoadChildNodes : true,
      documentTypes: this.documentTypes,
      mediaTypes: this.mediaTypes,
      dataTypes: this.dataTypes,
      templates: this.templates,
      partialViews: this.partialViews,
      stylesheets: this.stylesheets,
      scripts: this.scripts,
      languages: this.languages,
      dictionaryItems: this.dictionaryItems,
    };
  }
}
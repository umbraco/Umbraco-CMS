import {UserGroupPermissionBuilder} from './userGroupPermissionBuilder';
import {AliasHelper} from '../../helpers/AliasHelper';
import {UserGroupsPermissionsBaseBuilder} from './userGroupsPermissionsBaseBuilder';

export class UserGroupBuilder {
  name: string;
  icon: string;
  sections: string[];
  languages: string[];
  hasAccessToAllLanguages: boolean;
  documentStartNodeId: string;
  documentRootAccess: boolean;
  mediaStartNodeId: string;
  mediaRootAccess: boolean;
  fallbackPermissionsBuilder: UserGroupsPermissionsBaseBuilder;
  userGroupPermissionBuilder: UserGroupPermissionBuilder;
  description: string;

  constructor() {
    this.sections = [];
    this.languages = [];
  }

  withName(name: string) {
    this.name = name;
    return this;
  }

  withIcon(icon: string) {
    this.icon = icon;
    return this;
  }

  addSection(section: string) {
    this.sections.push(section);
    return this;
  }

  addLanguage(language: string) {
    this.languages.push(language);
    return this;
  }

  withHasAccessToAllLanguages(hasAccessToAllLanguages: boolean) {
    this.hasAccessToAllLanguages = hasAccessToAllLanguages;
    return this;
  }

  withDocumentStartNodeId(documentStartNodeId: string) {
    this.documentStartNodeId = documentStartNodeId;
    return this;
  }

  withDocumentRootAccess(documentRootAccess: boolean) {
    this.documentRootAccess = documentRootAccess;
    return this;
  }

  withMediaStartNodeId(mediaStartNodeId: string) {
    this.mediaStartNodeId = mediaStartNodeId;
    return this;
  }

  withMediaRootAccess(mediaRootAccess: boolean) {
    this.mediaRootAccess = mediaRootAccess;
    return this;
  }

  addFallbackPermission() {
    const builder = new UserGroupsPermissionsBaseBuilder(this);
    this.fallbackPermissionsBuilder = builder;
    return builder;
  }

  addPermissions() {
    const builder = new UserGroupPermissionBuilder(this);
    this.userGroupPermissionBuilder = builder;
    return builder;
  }

  withDescription(description: string) {
    this.description = description;
    return this;
  }

  build() {
    return {
      name: this.name || '',
      alias: AliasHelper.toAlias(this.name) || '',
      icon: this.icon || 'icon-bug',
      sections: this.sections || [],
      languages: this.languages || [],
      hasAccessToAllLanguages: this.hasAccessToAllLanguages || false,
      documentStartNode: this.documentStartNodeId ? {id: this.documentStartNodeId} : null,
      documentRootAccess: this.documentRootAccess || false,
      mediaStartNode: this.mediaStartNodeId ? {id: this.mediaStartNodeId} : null,
      mediaRootAccess: this.mediaRootAccess || false,
      fallbackPermissions: this.fallbackPermissionsBuilder ? this.fallbackPermissionsBuilder.build() : [],
      permissions: this.userGroupPermissionBuilder ? this.userGroupPermissionBuilder.build() : [],
      description: this.description || '',
    };
  }
}

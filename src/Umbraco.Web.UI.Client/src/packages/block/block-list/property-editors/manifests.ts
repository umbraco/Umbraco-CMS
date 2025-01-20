import { manifest as blockListTypeConfiguration } from './block-list-type-configuration/manifests.js';
import { manifests as blockListEditorManifests } from './block-list-editor/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [blockListTypeConfiguration, ...blockListEditorManifests];

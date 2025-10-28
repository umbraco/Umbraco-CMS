import { manifest as blockSingleTypeConfiguration } from './block-single-type-configuration/manifests.js';
import { manifests as blockSingleEditorManifests } from './block-single-editor/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [blockSingleTypeConfiguration, ...blockSingleEditorManifests];

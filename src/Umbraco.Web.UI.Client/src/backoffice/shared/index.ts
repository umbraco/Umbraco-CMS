import { manifests as componentManifests } from './components';
import { manifests as propertyActionManifests } from './property-actions/manifests';
import { manifests as propertyEditorManifests } from './property-editors/manifests';
import { manifests as collectionViewManifests } from './collection/views/manifests';
import { manifests as modalManifests } from './modals/manifests';
import type { UmbEntrypointOnInit } from '@umbraco-cms/extensions-api';
import { ManifestTypes } from '@umbraco-cms/extensions-registry';
import { UmbContextProviderController } from '@umbraco-cms/context-api';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/modal';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/notification';

export const manifests: Array<ManifestTypes> = [
	...componentManifests,
	...propertyActionManifests,
	...propertyEditorManifests,
	...collectionViewManifests,
	...modalManifests,
];

export const onInit: UmbEntrypointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);

	new UmbContextProviderController(_host, UMB_MODAL_CONTEXT_TOKEN, new UmbModalContext(_host));
	new UmbContextProviderController(_host, UMB_NOTIFICATION_CONTEXT_TOKEN, new UmbNotificationContext());
};

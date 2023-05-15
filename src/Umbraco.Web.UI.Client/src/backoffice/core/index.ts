import { manifests as componentManifests, UmbStoreExtensionInitializer } from './components';
import { manifests as propertyActionManifests } from './property-actions/manifests';
import { manifests as propertyEditorManifests } from './property-editors/manifests';
import { manifests as modalManifests } from './modals/manifests';

import { UmbBackofficeNotificationContainerElement } from './components/backoffice-frame/backoffice-notification-container.element';
import { UmbBackofficeModalContainerElement } from './components/backoffice-frame/backoffice-modal-container.element';

import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extensions-api';
import { ManifestKind, ManifestTypes } from '@umbraco-cms/backoffice/extensions-registry';

import './notification';

export const manifests: Array<ManifestTypes | ManifestKind> = [
	...componentManifests,
	...propertyActionManifests,
	...propertyEditorManifests,
	...modalManifests,
];

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {
	new UmbStoreExtensionInitializer(host);

	extensionRegistry.registerMany(manifests);

	const notificationContainerElement = new UmbBackofficeNotificationContainerElement();
	host.appendChild(notificationContainerElement);

	const modalContainerElement = new UmbBackofficeModalContainerElement();
	host.appendChild(modalContainerElement);

	new UmbContextProviderController(host, UMB_NOTIFICATION_CONTEXT_TOKEN, new UmbNotificationContext());
	new UmbContextProviderController(host, UMB_MODAL_CONTEXT_TOKEN, new UmbModalContext(host));
};

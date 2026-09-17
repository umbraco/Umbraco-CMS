import { UmbCreateElementCollectionActionElement } from './create-element-collection-action.element.js';
import { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbCurrentUserContext, UmbCurrentUserStore } from '@umbraco-cms/backoffice/current-user';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UmbAncestorsEntityContext, UmbEntityContext, type UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { ignoreResizeObserverLoopErrors } from '@umbraco-cms/internal/test-utils';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { manifests as userPermissionConditionManifests } from '../../user-permissions/conditions/manifests.js';

@customElement('test-create-element-collection-action-host')
class UmbTestCreateElementCollectionActionHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	currentUserContext = new UmbCurrentUserContext(this);
	entityContext = new UmbEntityContext(this);
	ancestorsContext = new UmbAncestorsEntityContext(this);

	constructor() {
		super();
		new UmbNotificationContext(this);
		new UmbCurrentUserStore(this);
	}

	async init() {
		await this.currentUserContext.load();
	}

	setEntity(entity: UmbEntityModel) {
		this.entityContext.setUnique(entity.unique);
		this.entityContext.setEntityType(entity.entityType);
		this.ancestorsContext.setAncestors([]);
	}
}

/**
 * The permission fixtures below come from the `userPermissions` mock set:
 * `permissions-folder-read-only-id` grants `Umb.Element.Create`, `permissions-element-read-only-id` does not.
 */
describe('UmbCreateElementCollectionActionElement', () => {
	let host: UmbTestCreateElementCollectionActionHostElement;
	let element: UmbCreateElementCollectionActionElement;
	let restoreErrorHandler: () => void;

	const anyButton = () => element.shadowRoot!.querySelector('uui-button');

	const renderForFolder = async (unique: string) => {
		host.setEntity({ unique, entityType: UMB_ELEMENT_FOLDER_ENTITY_TYPE });

		element = document.createElement('umb-create-element-collection-action') as UmbCreateElementCollectionActionElement;
		host.appendChild(element);

		// The allowed element types are fetched only once the create permission resolves.
		await aTimeout(100);
		await element.updateComplete;
	};

	before(async () => {
		await useMockSet('userPermissions');
		umbExtensionsRegistry.registerMany(userPermissionConditionManifests);
	});

	after(() => {
		userPermissionConditionManifests.forEach((manifest) => umbExtensionsRegistry.unregister(manifest.alias));
	});

	beforeEach(async () => {
		restoreErrorHandler = ignoreResizeObserverLoopErrors();
		host = new UmbTestCreateElementCollectionActionHostElement();
		document.body.appendChild(host);
		await host.init();
	});

	afterEach(() => {
		document.body.innerHTML = '';
		restoreErrorHandler();
	});

	describe('when the user may create elements', () => {
		beforeEach(async () => {
			await renderForFolder('permissions-folder-read-only-id');
		});

		it('offers the allowed element types', () => {
			expect(anyButton()).to.exist;
			expect(anyButton()!.hasAttribute('disabled')).to.be.false;
		});
	});

	describe('when the user may not create elements', () => {
		beforeEach(async () => {
			await renderForFolder('permissions-element-read-only-id');
		});

		it('renders no button at all, not even a disabled one', () => {
			expect(anyButton()).to.not.exist;
		});
	});
});

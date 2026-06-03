import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbCurrentUserContext, UmbCurrentUserStore } from '@umbraco-cms/backoffice/current-user';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UmbAncestorsEntityContext, UmbEntityContext, type UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';
import { UmbElementOrElementFolderUserPermissionCondition } from './element-or-element-folder-user-permission.condition.js';
import { UMB_ELEMENT_OR_ELEMENT_FOLDER_USER_PERMISSION_CONDITION_ALIAS } from './constants.js';
import { UMB_USER_PERMISSION_ELEMENT_READ } from '../constants.js';
import { UMB_USER_PERMISSION_ELEMENT_FOLDER_READ } from '../../folder/user-permissions/constants.js';
import { UMB_ELEMENT_ENTITY_TYPE } from '../../entity.js';

@customElement('test-controller-host-combined-permission')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {
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
	}

	setEntityAncestors(ancestors: Array<UmbEntityModel>) {
		this.ancestorsContext.setAncestors(ancestors);
	}
}

const CONDITION_CONFIG = {
	alias: UMB_ELEMENT_OR_ELEMENT_FOLDER_USER_PERMISSION_CONDITION_ALIAS,
	element: { allOf: [UMB_USER_PERMISSION_ELEMENT_READ] },
	folder: { allOf: [UMB_USER_PERMISSION_ELEMENT_FOLDER_READ] },
};

describe('UmbElementOrElementFolderUserPermissionCondition', () => {
	let hostElement: UmbTestControllerHostElement;
	let condition: UmbElementOrElementFolderUserPermissionCondition;

	before(async () => {
		await useMockSet('userPermissions');
	});

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);
		await hostElement.init();
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('Element read only', () => {
		it('should be permitted when user has element read but not folder read', (done) => {
			hostElement.setEntity({ unique: 'permissions-element-read-only-id', entityType: UMB_ELEMENT_ENTITY_TYPE });
			hostElement.setEntityAncestors([]);

			condition = new UmbElementOrElementFolderUserPermissionCondition(hostElement, {
				host: hostElement,
				config: CONDITION_CONFIG,
				onChange: () => {
					expect(condition.permitted).to.be.true;
					condition.hostDisconnected();
					done();
				},
			});
		});
	});

	describe('Folder read only', () => {
		it('should be permitted when user has folder read but not element read', (done) => {
			hostElement.setEntity({ unique: 'permissions-folder-read-only-id', entityType: UMB_ELEMENT_ENTITY_TYPE });
			hostElement.setEntityAncestors([]);

			condition = new UmbElementOrElementFolderUserPermissionCondition(hostElement, {
				host: hostElement,
				config: CONDITION_CONFIG,
				onChange: () => {
					expect(condition.permitted).to.be.true;
					condition.hostDisconnected();
					done();
				},
			});
		});
	});

	describe('Both read', () => {
		it('should be permitted when user has both element read and folder read', (done) => {
			hostElement.setEntity({ unique: 'permissions-both-read-id', entityType: UMB_ELEMENT_ENTITY_TYPE });
			hostElement.setEntityAncestors([]);

			condition = new UmbElementOrElementFolderUserPermissionCondition(hostElement, {
				host: hostElement,
				config: CONDITION_CONFIG,
				onChange: () => {
					expect(condition.permitted).to.be.true;
					condition.hostDisconnected();
					done();
				},
			});
		});
	});

	describe('Neither read', () => {
		it('should not be permitted when user has neither element read nor folder read', (done) => {
			hostElement.setEntity({ unique: 'permissions-neither-read-id', entityType: UMB_ELEMENT_ENTITY_TYPE });
			hostElement.setEntityAncestors([]);

			condition = new UmbElementOrElementFolderUserPermissionCondition(hostElement, {
				host: hostElement,
				config: CONDITION_CONFIG,
				onChange: () => {
					// onChange fires only when permitted transitions; if it ever transitions to true that is a failure
					if (condition.permitted) {
						condition.hostDisconnected();
						done(new Error('Expected condition to remain not permitted'));
					}
				},
			});

			// Safe to assert after 200ms: mock contexts resolve synchronously, so both
			// sub-conditions have evaluated well within this window. The initial default
			// is also false, so a pass here confirms the condition never became true.
			setTimeout(() => {
				expect(condition.permitted).to.be.false;
				condition.hostDisconnected();
				done();
			}, 200);
		});
	});
});

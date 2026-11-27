import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbCurrentUserContext, UmbCurrentUserStore } from '@umbraco-cms/backoffice/current-user';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UmbDocumentUserPermissionCondition } from './document-user-permission.condition';
import { UmbAncestorsEntityContext, UmbEntityContext, type UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import {
	UMB_DOCUMENT_ENTITY_TYPE,
	UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE,
	UMB_DOCUMENT_ROOT_ENTITY_TYPE,
	UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_DOCUMENT_CREATE,
	UMB_USER_PERMISSION_DOCUMENT_DELETE,
	UMB_USER_PERMISSION_DOCUMENT_READ,
	UMB_USER_PERMISSION_DOCUMENT_UPDATE,
} from '@umbraco-cms/backoffice/document';
import type { UmbDocumentUserPermissionConditionConfig } from './types.js';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';

@customElement('test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	currentUserContext = new UmbCurrentUserContext(this);
	entityContext = new UmbEntityContext(this);
	ancestorsContext = new UmbAncestorsEntityContext(this);
	currentUserStore = new UmbCurrentUserStore(this);

	constructor() {
		super();
		new UmbNotificationContext(this);
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

	setDocumentStartNodeAccess(hasDocumentRootAccess: boolean, documentStartNodeUniques: Array<string>) {
		this.currentUserStore.update({
			hasDocumentRootAccess,
			documentStartNodeUniques: documentStartNodeUniques.map((unique) => ({ unique })),
		});
	}

	setPermissions(
		fallbackPermissions: Array<string>,
		documentPermissions: Array<{ unique: string; verbs: Array<string> }>,
	) {
		this.currentUserStore.update({
			fallbackPermissions,
			permissions: documentPermissions.map(({ unique, verbs }) => ({
				$type: 'DocumentPermissionPresentationModel',
				document: { id: unique },
				verbs,
			})),
		});
	}
}

describe('UmbDocumentUserPermissionCondition', () => {
	let hostElement: UmbTestControllerHostElement;
	let condition: UmbDocumentUserPermissionCondition;

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

	describe('Specific permissions', () => {
		it('should return true if a user has permissions', (done) => {
			// Sets the current entity data
			hostElement.setEntity({
				unique: 'permissions-document-id',
				entityType: UMB_DOCUMENT_ENTITY_TYPE,
			});

			// This entity does not have any ancestors.
			hostElement.setEntityAncestors([]);

			let callbackCount = 0;

			// We expect to find the read permission on the current entity
			condition = new UmbDocumentUserPermissionCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
					allOf: [UMB_USER_PERMISSION_DOCUMENT_READ],
				},
				onChange: () => {
					callbackCount++;
					if (callbackCount === 1) {
						expect(condition.permitted).to.be.true;
						condition.hostDisconnected();
						done();
					}
				},
			});
		});
	});

	describe('Inherited permissions', () => {
		it('should inherit permissions from closest ancestor with specific permissions set', (done) => {
			// Sets the current entity data
			hostElement.setEntity({
				unique: 'permissions-document-1-id',
				entityType: UMB_DOCUMENT_ENTITY_TYPE,
			});

			// Sets the ancestors of the current entity. These are the ancestors that will be checked for permissions.
			hostElement.setEntityAncestors([{ unique: 'permissions-document-id', entityType: UMB_DOCUMENT_ENTITY_TYPE }]);

			let callbackCount = 0;

			// We expect to find the read permission on the ancestor
			condition = new UmbDocumentUserPermissionCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
					allOf: [UMB_USER_PERMISSION_DOCUMENT_READ],
				},
				onChange: () => {
					callbackCount++;
					if (callbackCount === 1) {
						expect(condition.permitted).to.be.true;
						condition.hostDisconnected();
						done();
					}
				},
			});
		});
	});

	describe('Fallback Permissions', () => {
		it('should use the fallback permissions if no specific permissions are set for the entity or ancestors', (done) => {
			// Sets the current entity to a document without permissions
			hostElement.setEntity({
				unique: 'no-permissions-document-id',
				entityType: UMB_DOCUMENT_ENTITY_TYPE,
			});

			// Sets the ancestors of the current entity. These are the ancestors that will be checked for permissions.
			// This ancestor does not have any permissions either.
			hostElement.setEntityAncestors([
				{ unique: 'no-permissions-parent-document-id', entityType: UMB_DOCUMENT_ENTITY_TYPE },
			]);

			let callbackCount = 0;

			// We expect to find the read permission in the fallback permissions
			condition = new UmbDocumentUserPermissionCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
					allOf: [UMB_USER_PERMISSION_DOCUMENT_READ],
				},
				onChange: () => {
					callbackCount++;
					if (callbackCount === 1) {
						expect(condition.permitted).to.be.true;
						condition.hostDisconnected();
						done();
					}
				},
			});
		});
	});

	describe('Start node access', () => {
		it('is permitted when the document is within the users start-node subtree', (done) => {
			// User without root access, scoped to the "permissions-document-id" subtree
			hostElement.setDocumentStartNodeAccess(false, ['permissions-document-id']);

			// A descendant of the start node
			hostElement.setEntity({
				unique: 'permissions-document-1-id',
				entityType: UMB_DOCUMENT_ENTITY_TYPE,
			});
			hostElement.setEntityAncestors([{ unique: 'permissions-document-id', entityType: UMB_DOCUMENT_ENTITY_TYPE }]);

			let callbackCount = 0;

			condition = new UmbDocumentUserPermissionCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
					allOf: [UMB_USER_PERMISSION_DOCUMENT_READ],
				},
				onChange: () => {
					callbackCount++;
					if (callbackCount === 1) {
						expect(condition.permitted).to.be.true;
						condition.hostDisconnected();
						done();
					}
				},
			});
		});

		it('is not permitted when the document is outside the users start-node subtree', (done) => {
			// User without root access, scoped to the "permissions-document-id" subtree
			hostElement.setDocumentStartNodeAccess(false, ['permissions-document-id']);

			// A document outside the start-node subtree - fallback permissions would otherwise grant read
			hostElement.setEntity({
				unique: 'no-permissions-document-id',
				entityType: UMB_DOCUMENT_ENTITY_TYPE,
			});
			hostElement.setEntityAncestors([
				{ unique: 'no-permissions-parent-document-id', entityType: UMB_DOCUMENT_ENTITY_TYPE },
			]);

			condition = new UmbDocumentUserPermissionCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
					allOf: [UMB_USER_PERMISSION_DOCUMENT_READ],
				},
				onChange: () => {},
			});

			// The onChange callback is not called when the condition is false, so we need to wait and check manually
			setTimeout(() => {
				expect(condition.permitted).to.be.false;
				condition.hostDisconnected();
				done();
			}, 200);
		});

		it('is not permitted when the user has no start nodes and no root access', (done) => {
			hostElement.setDocumentStartNodeAccess(false, []);

			hostElement.setEntity({
				unique: 'permissions-document-id',
				entityType: UMB_DOCUMENT_ENTITY_TYPE,
			});
			hostElement.setEntityAncestors([]);

			condition = new UmbDocumentUserPermissionCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
					allOf: [UMB_USER_PERMISSION_DOCUMENT_READ],
				},
				onChange: () => {},
			});

			// The onChange callback is not called when the condition is false, so we need to wait and check manually
			setTimeout(() => {
				expect(condition.permitted).to.be.false;
				condition.hostDisconnected();
				done();
			}, 200);
		});

		it('is permitted with ignorerUserStartNodes:true even when outside start-node subtree', (done) => {
			hostElement.setDocumentStartNodeAccess(false, ['permissions-document-id']);
			hostElement.setEntity({ unique: 'no-permissions-document-id', entityType: UMB_DOCUMENT_ENTITY_TYPE });
			hostElement.setEntityAncestors([
				{ unique: 'no-permissions-parent-document-id', entityType: UMB_DOCUMENT_ENTITY_TYPE },
			]);

			let callbackCount = 0;

			condition = new UmbDocumentUserPermissionCondition(hostElement, {
				host: hostElement,
				config: {
					alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
					allOf: [UMB_USER_PERMISSION_DOCUMENT_READ],
					ignorerUserStartNodes: true, // bypass start-node check
				},
				onChange: () => {
					callbackCount++;
					if (callbackCount === 1) {
						expect(condition.permitted).to.be.true;
						condition.hostDisconnected();
						done();
					}
				},
			});
		});
	});

	describe('Rules', () => {
		const ALL_VERBS = [
			UMB_USER_PERMISSION_DOCUMENT_READ,
			UMB_USER_PERMISSION_DOCUMENT_CREATE,
			UMB_USER_PERMISSION_DOCUMENT_UPDATE,
			UMB_USER_PERMISSION_DOCUMENT_DELETE,
		];

		// The condition starts as not permitted and only reports changes, so a denial is read after it has settled.
		function getPermitted(config: Omit<UmbDocumentUserPermissionConditionConfig, 'alias'>): Promise<boolean> {
			return new Promise((resolve) => {
				const condition = new UmbDocumentUserPermissionCondition(hostElement, {
					host: hostElement,
					config: { alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS, ...config },
					onChange: () => {},
				});
				setTimeout(() => {
					resolve(condition.permitted);
					condition.hostDisconnected();
				}, 100);
			});
		}

		function setDocument(unique: string, ancestorUniques: Array<string> = []) {
			hostElement.setEntity({ unique, entityType: UMB_DOCUMENT_ENTITY_TYPE });
			hostElement.setEntityAncestors(
				ancestorUniques.map((ancestorUnique) => ({ unique: ancestorUnique, entityType: UMB_DOCUMENT_ENTITY_TYPE })),
			);
		}

		describe('Per-document permissions', () => {
			it('restricts the document when its entry has fewer verbs than the fallback', async () => {
				hostElement.setPermissions(ALL_VERBS, [{ unique: 'restricted', verbs: [UMB_USER_PERMISSION_DOCUMENT_READ] }]);
				setDocument('restricted');

				expect(await getPermitted({ allOf: [UMB_USER_PERMISSION_DOCUMENT_DELETE] })).to.be.false;
			});

			it('restricts descendants of the document', async () => {
				hostElement.setPermissions(ALL_VERBS, [{ unique: 'restricted', verbs: [UMB_USER_PERMISSION_DOCUMENT_READ] }]);
				setDocument('restricted-grandchild', ['restricted', 'restricted-child']);

				expect(await getPermitted({ allOf: [UMB_USER_PERMISSION_DOCUMENT_DELETE] })).to.be.false;
			});

			it('does not restrict documents outside the entry', async () => {
				hostElement.setPermissions(ALL_VERBS, [{ unique: 'restricted', verbs: [UMB_USER_PERMISSION_DOCUMENT_READ] }]);
				setDocument('sibling', ['parent']);

				expect(await getPermitted({ allOf: [UMB_USER_PERMISSION_DOCUMENT_DELETE] })).to.be.true;
			});

			it('replaces the fallback even when the entry has no verbs', async () => {
				hostElement.setPermissions(ALL_VERBS, [{ unique: 'restricted', verbs: [] }]);
				setDocument('restricted');

				expect(await getPermitted({ allOf: [UMB_USER_PERMISSION_DOCUMENT_READ] })).to.be.false;
			});

			it('grants verbs the fallback does not have', async () => {
				hostElement.setPermissions(
					[UMB_USER_PERMISSION_DOCUMENT_READ],
					[{ unique: 'granted', verbs: [UMB_USER_PERMISSION_DOCUMENT_READ, UMB_USER_PERMISSION_DOCUMENT_DELETE] }],
				);
				setDocument('granted');

				expect(await getPermitted({ allOf: [UMB_USER_PERMISSION_DOCUMENT_DELETE] })).to.be.true;
			});

			it('uses the nearest entry when a nearer entry grants more', async () => {
				hostElement.setPermissions(
					[UMB_USER_PERMISSION_DOCUMENT_READ],
					[
						{ unique: 'outer', verbs: [UMB_USER_PERMISSION_DOCUMENT_READ] },
						{ unique: 'inner', verbs: [UMB_USER_PERMISSION_DOCUMENT_READ, UMB_USER_PERMISSION_DOCUMENT_DELETE] },
					],
				);
				setDocument('document', ['outer', 'inner']);

				expect(await getPermitted({ allOf: [UMB_USER_PERMISSION_DOCUMENT_DELETE] })).to.be.true;
			});

			it('uses the nearest entry when a nearer entry grants less', async () => {
				hostElement.setPermissions(ALL_VERBS, [
					{ unique: 'outer', verbs: [UMB_USER_PERMISSION_DOCUMENT_READ, UMB_USER_PERMISSION_DOCUMENT_DELETE] },
					{ unique: 'inner', verbs: [UMB_USER_PERMISSION_DOCUMENT_READ] },
				]);
				setDocument('document', ['outer', 'inner']);

				expect(await getPermitted({ allOf: [UMB_USER_PERMISSION_DOCUMENT_DELETE] })).to.be.false;
			});
		});

		describe('Roots', () => {
			it('uses the fallback for the content root when the user has root access', async () => {
				hostElement.setPermissions(ALL_VERBS, [{ unique: 'restricted', verbs: [] }]);
				hostElement.setEntity({ unique: null, entityType: UMB_DOCUMENT_ROOT_ENTITY_TYPE });
				hostElement.setEntityAncestors([]);

				expect(await getPermitted({ allOf: [UMB_USER_PERMISSION_DOCUMENT_CREATE] })).to.be.true;
			});

			it('is not permitted for the content root without root access', async () => {
				hostElement.setPermissions(ALL_VERBS, []);
				hostElement.setDocumentStartNodeAccess(false, ['start-node']);
				hostElement.setEntity({ unique: null, entityType: UMB_DOCUMENT_ROOT_ENTITY_TYPE });
				hostElement.setEntityAncestors([]);

				expect(await getPermitted({ allOf: [UMB_USER_PERMISSION_DOCUMENT_CREATE] })).to.be.false;
			});

			it('uses the fallback for the recycle bin root when the user has root access', async () => {
				hostElement.setPermissions([UMB_USER_PERMISSION_DOCUMENT_READ], []);
				hostElement.setEntity({ unique: null, entityType: UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE });
				hostElement.setEntityAncestors([]);

				expect(await getPermitted({ allOf: [UMB_USER_PERMISSION_DOCUMENT_READ] })).to.be.true;
			});

			it('is not permitted for the recycle bin root without root access', async () => {
				hostElement.setPermissions([UMB_USER_PERMISSION_DOCUMENT_READ], []);
				hostElement.setDocumentStartNodeAccess(false, ['start-node']);
				hostElement.setEntity({ unique: null, entityType: UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE });
				hostElement.setEntityAncestors([]);

				expect(await getPermitted({ allOf: [UMB_USER_PERMISSION_DOCUMENT_READ] })).to.be.false;
			});
		});

		describe('Entity', () => {
			it('is not permitted before the entity is known', async () => {
				hostElement.setPermissions(ALL_VERBS, []);

				expect(await getPermitted({ allOf: [UMB_USER_PERMISSION_DOCUMENT_READ] })).to.be.false;
			});
		});

		describe('allOf and oneOf', () => {
			const READ = UMB_USER_PERMISSION_DOCUMENT_READ;
			const UPDATE = UMB_USER_PERMISSION_DOCUMENT_UPDATE;
			const DELETE = UMB_USER_PERMISSION_DOCUMENT_DELETE;
			const CREATE = UMB_USER_PERMISSION_DOCUMENT_CREATE;

			const cases: Array<{ config: Omit<UmbDocumentUserPermissionConditionConfig, 'alias'>; expected: boolean }> = [
				{ config: { allOf: [READ, UPDATE] }, expected: true },
				{ config: { allOf: [READ, DELETE] }, expected: false },
				{ config: { oneOf: [DELETE, UPDATE] }, expected: true },
				{ config: { oneOf: [DELETE, CREATE] }, expected: false },
				{ config: { allOf: [READ], oneOf: [DELETE, UPDATE] }, expected: true },
				{ config: { allOf: [READ], oneOf: [DELETE] }, expected: false },
				{ config: { allOf: [DELETE], oneOf: [READ] }, expected: false },
			];

			for (const { config, expected } of cases) {
				it(`is ${expected ? '' : 'not '}permitted for ${JSON.stringify(config)} when the user has Read and Update`, async () => {
					hostElement.setPermissions([READ, UPDATE], []);
					setDocument('document');

					expect(await getPermitted(config)).to.equal(expected);
				});
			}
		});

		describe('Current user changes', () => {
			it('updates when the current user is reloaded with other permissions', (done) => {
				hostElement.setPermissions(ALL_VERBS, []);
				setDocument('document');

				const changes: Array<boolean> = [];
				const condition = new UmbDocumentUserPermissionCondition(hostElement, {
					host: hostElement,
					config: { alias: UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS, allOf: [UMB_USER_PERMISSION_DOCUMENT_DELETE] },
					onChange: (permitted) => {
						changes.push(permitted);
						if (changes.length === 1) {
							hostElement.setPermissions([UMB_USER_PERMISSION_DOCUMENT_READ], []);
						} else {
							expect(changes).to.deep.equal([true, false]);
							condition.hostDisconnected();
							done();
						}
					},
				});
			});
		});
	});
});

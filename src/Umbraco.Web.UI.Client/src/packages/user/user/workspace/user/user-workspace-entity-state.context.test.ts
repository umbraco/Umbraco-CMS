import { UmbUserWorkspaceContext } from './user-workspace.context.js';
import { UmbUserStateEnum } from '../../types.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('umb-test-user-workspace-host')
class UmbTestUserWorkspaceHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbUserWorkspaceContext (entityState)', () => {
	let context: UmbUserWorkspaceContext;

	beforeEach(() => {
		const hostElement = new UmbTestUserWorkspaceHostElement();
		context = new UmbUserWorkspaceContext(hostElement);
	});

	it('pushes an Active entry with a positive look', () => {
		context.updateProperty('state', UmbUserStateEnum.ACTIVE);
		const states = context.entityState.getStates();
		expect(states).to.have.lengthOf(1);
		expect(states[0]).to.deep.include({ label: '#user_stateActive', look: 'positive', weight: 10 });
	});

	it('pushes no entry for the All pseudo-value', () => {
		context.updateProperty('state', UmbUserStateEnum.ALL);
		expect(context.entityState.getStates()).to.have.lengthOf(0);
	});

	it('pushes a Disabled entry with a danger look', () => {
		context.updateProperty('state', UmbUserStateEnum.DISABLED);
		const states = context.entityState.getStates();
		expect(states).to.have.lengthOf(1);
		expect(states[0]).to.deep.include({ label: '#user_stateDisabled', look: 'danger', weight: 50 });
	});

	it('pushes a Locked out entry with a danger look', () => {
		context.updateProperty('state', UmbUserStateEnum.LOCKED_OUT);
		const states = context.entityState.getStates();
		expect(states[0]).to.deep.include({ label: '#user_stateLockedOut', look: 'danger', weight: 50 });
	});

	it('pushes an Invited entry with a warning look', () => {
		context.updateProperty('state', UmbUserStateEnum.INVITED);
		const states = context.entityState.getStates();
		expect(states[0]).to.deep.include({ label: '#user_stateInvited', look: 'warning', weight: 20 });
	});

	it('pushes an Inactive entry with a warning look', () => {
		context.updateProperty('state', UmbUserStateEnum.INACTIVE);
		const states = context.entityState.getStates();
		expect(states[0]).to.deep.include({ label: '#user_stateInactive', look: 'warning', weight: 20 });
	});

	it('replaces the entry rather than duplicating it when the state changes', () => {
		context.updateProperty('state', UmbUserStateEnum.DISABLED);
		context.updateProperty('state', UmbUserStateEnum.LOCKED_OUT);

		const states = context.entityState.getStates();
		expect(states).to.have.lengthOf(1);
		expect(states[0]).to.deep.include({ label: '#user_stateLockedOut' });
	});

	it('removes the entry entirely once the state becomes falsy', () => {
		context.updateProperty('state', UmbUserStateEnum.DISABLED);
		context.updateProperty('state', null as never);

		expect(context.entityState.getStates()).to.have.lengthOf(0);
	});
});

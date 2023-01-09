import { html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
//import type { UmbPropertyActionMenuContext } from '../shared/property-action-menu/property-action-menu.context';
import { UmbPropertyAction } from '../shared/property-action/property-action.model';
import type { UmbWorkspacePropertyContext } from '../../components/workspace-property/workspace-property.context';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-property-action-clear')
export class UmbPropertyActionClearElement extends UmbLitElement implements UmbPropertyAction {
	
	@property()
	value = '';

	// THESE OUT COMMENTED CODE IS USED FOR THE EXAMPLE BELOW, TODO: Should be transferred to some documentation.
	//private _propertyActionMenuContext?: UmbPropertyActionMenuContext;
	private _propertyContext?: UmbWorkspacePropertyContext;

	constructor() {
		super();

		/*
		this.consumeContext('umbPropertyActionMenu', (propertyActionsContext: UmbPropertyActionMenuContext) => {
			this._propertyActionMenuContext = propertyActionsContext;
		});
		*/
		this.consumeContext('umbPropertyContext', (propertyContext: UmbWorkspacePropertyContext) => {
			this._propertyContext = propertyContext;
		});
	}

	private _handleLabelClick() {
		this._clearValue();
		this.dispatchEvent(new CustomEvent('close', { bubbles: true, composed: true }));
		// Or you can do this:
		//this._propertyActionMenuContext?.close();
	}

	private _clearValue() {
		// TODO: how do we want to update the value? Testing an event based approach. We need to test an api based approach too.
		//this.value = '';
		//this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
		// Or you can do this:
		this._propertyContext?.resetValue('');
	}

	render() {
		return html` <uui-menu-item label="Clear" @click-label="${this._handleLabelClick}">
			<uui-icon slot="icon" name="delete"></uui-icon>
		</uui-menu-item>`;
	}
}

export default UmbPropertyActionClearElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-action-clear': UmbPropertyActionClearElement;
	}
}

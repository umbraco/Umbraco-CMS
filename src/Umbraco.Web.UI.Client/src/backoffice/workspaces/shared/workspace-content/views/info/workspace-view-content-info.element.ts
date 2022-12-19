import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { distinctUntilChanged } from 'rxjs';
import { UmbNodeContext } from '../../node.context';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { DocumentDetails, MediaDetails } from '@umbraco-cms/models';

@customElement('umb-workspace-view-content-info')
export class UmbWorkspaceViewContentInfoElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display:block;
				margin: var(--uui-size-layout-1);
			}
		`
	];

	private _nodeContext?: UmbNodeContext;

	@state()
	private _nodeName = '';

	constructor() {
		super();

		this.consumeContext('umbNodeContext', (nodeContext) => {
			this._nodeContext = nodeContext;
			this._observeNode();
		});
	}

	private _observeNode() {
		if (!this._nodeContext) return;

		this.observe<DocumentDetails | MediaDetails>(this._nodeContext.data.pipe(distinctUntilChanged()), (node) => {
			this._nodeName = node.name as string;
		});
	}

	render() {
		return html`<div>Info Workspace View for ${this._nodeName}</div>`;
	}
}

export default UmbWorkspaceViewContentInfoElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-content-info': UmbWorkspaceViewContentInfoElement;
	}
}

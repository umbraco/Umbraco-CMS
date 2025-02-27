import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from '../user-group/constants.js';
import type { ManifestUserPermission, UmbContextualUserPermissionModel } from './types.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UUIBooleanInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, property, type PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-hello')
export class UmbHelloElement extends UmbLitElement {
	@property({ type: Object, attribute: false })
	manifest?: ManifestUserPermission;

	#onChange(event: UUIBooleanInputEvent) {
		event.stopPropagation();

		if (this.manifest === undefined) {
			throw new Error('Manifest is undefined');
		}

		const permissions = this.#context?.getPermissions();
		const permission: UmbContextualUserPermissionModel = {
			$type: 'UnknownTypePermissionPresentationModel',
			context: this.manifest.meta.permission.context,
			verbs: this.manifest.meta.permission.verbs,
		};

		this.#context?.setPermissions([...permissions, permission]);
	}

	#context?: typeof UMB_USER_GROUP_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_USER_GROUP_WORKSPACE_CONTEXT, (context) => {
			this.#context = context;
		});
	}

	protected override firstUpdated(_changedProperties: PropertyValues): void {
		super.firstUpdated(_changedProperties);
	}

	override render() {
		const label = this.manifest?.meta.label ?? this.manifest?.name ?? '';
		const description = this.manifest?.meta.description ?? '';
		return html`<div id="setting">
			<uui-toggle label=${label} @change=${this.#onChange}>
				<div id="meta">
					<div id="name">${label}</div>
					<small>${description}</small>
				</div>
			</uui-toggle>
		</div>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#setting {
				display: flex;
				align-items: center;
				border-bottom: 1px solid var(--uui-color-divider);
				padding: var(--uui-size-space-3) 0 var(--uui-size-space-4) 0;
			}

			#meta {
				margin-left: var(--uui-size-space-4);
				line-height: 1.2em;
			}

			#name {
				font-weight: bold;
			}
		`,
	];
}

export { UmbHelloElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-hello': UmbHelloElement;
	}
}

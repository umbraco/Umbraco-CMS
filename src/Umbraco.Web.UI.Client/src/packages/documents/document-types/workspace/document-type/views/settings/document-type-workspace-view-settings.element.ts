import { UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT } from '../../document-type-workspace.context-token.js';
import { css, html, customElement, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UUIBooleanInputEvent, UUIToggleElement } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-document-type-workspace-view-settings')
export class UmbDocumentTypeWorkspaceViewSettingsElement extends UmbLitElement implements UmbWorkspaceViewElement {
	#workspaceContext?: typeof UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _variesByCulture?: boolean;

	@state()
	private _variesBySegment?: boolean;

	@state()
	private _isElement?: boolean;

	@state()
	private _keepAllVersionsNewerThanDays?: number | null;

	@state()
	private _keepLatestVersionPerDayForDays?: number | null;

	@state()
	private _preventCleanup?: boolean;

	constructor() {
		super();

		// TODO: Figure out if this is the best way to consume the context or if it can be strongly typed with an UmbContextToken
		this.consumeContext(UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT, (documentTypeContext) => {
			this.#workspaceContext = documentTypeContext;
			this.#observeDocumentType();
		});
	}

	#observeDocumentType() {
		if (!this.#workspaceContext) return;
		this.observe(
			this.#workspaceContext.variesByCulture,
			(variesByCulture) => (this._variesByCulture = variesByCulture),
		);
		this.observe(
			this.#workspaceContext.variesBySegment,
			(variesBySegment) => (this._variesBySegment = variesBySegment),
		);
		this.observe(this.#workspaceContext.isElement, (isElement) => (this._isElement = isElement));

		this.observe(this.#workspaceContext.cleanup, (cleanup) => {
			this._preventCleanup = cleanup?.preventCleanup;
			this._keepAllVersionsNewerThanDays = cleanup?.keepAllVersionsNewerThanDays;
			this._keepLatestVersionPerDayForDays = cleanup?.keepLatestVersionPerDayForDays;
		});
	}

	#setCleanup() {
		this.#workspaceContext?.setCleanup({
			preventCleanup: this._preventCleanup ?? false,
			keepAllVersionsNewerThanDays: this._keepAllVersionsNewerThanDays,
			keepLatestVersionPerDayForDays: this._keepLatestVersionPerDayForDays,
		});
	}

	#onChangePreventCleanup(event: UUIBooleanInputEvent) {
		this._preventCleanup = event.target.checked;
		if (this._preventCleanup) {
			this._keepAllVersionsNewerThanDays = null;
			this._keepLatestVersionPerDayForDays = null;
		}
		this.#setCleanup();
	}

	#onChangeKeepAllVersionsNewerThanDays(event: Event & { target: HTMLInputElement }) {
		this._keepAllVersionsNewerThanDays = parseInt(event.target.value);
		this.#setCleanup();
	}

	#onChangeKeepLatestVersionPerDayForDays(event: Event & { target: HTMLInputElement }) {
		this._keepLatestVersionPerDayForDays = parseInt(event.target.value);
		this.#setCleanup();
	}

	override render() {
		return html`
			<uui-box headline="Data variations">
				<umb-property-layout
					alias="VaryByCulture"
					label=${this.localize.term('contentTypeEditor_cultureVariantHeading')}>
					<div slot="description">
						<umb-localize key="contentTypeEditor_cultureVariantDescription"
							>Allow editors to create content of different languages.</umb-localize
						>
					</div>
					<div slot="editor">
						<uui-toggle
							?checked=${this._variesByCulture}
							@change=${(e: CustomEvent) => {
								this.#workspaceContext?.setVariesByCulture((e.target as UUIToggleElement).checked);
							}}
							label=${this.localize.term('contentTypeEditor_cultureVariantLabel')}></uui-toggle>
					</div>
				</umb-property-layout>
				<umb-property-layout
					alias="VaryBySegments"
					label=${this.localize.term('contentTypeEditor_segmentVariantHeading')}>
					<div slot="description">
						<umb-localize key="contentTypeEditor_segmentVariantDescription"
							>Allow editors to segment their content.</umb-localize
						>
					</div>
					<div slot="editor">
						<uui-toggle
							?checked=${this._variesBySegment}
							@change=${(e: CustomEvent) => {
								this.#workspaceContext?.setVariesBySegment((e.target as UUIToggleElement).checked);
							}}
							label=${this.localize.term('contentTypeEditor_segmentVariantLabel')}></uui-toggle>
					</div>
				</umb-property-layout>
				<umb-property-layout alias="ElementType" label=${this.localize.term('contentTypeEditor_elementHeading')}>
					<div slot="description">
						<umb-localize key="contentTypeEditor_elementDescription"
							>An Element Type is used for content instances in Property Editors, like the Block Editors.</umb-localize
						>
					</div>
					<div slot="editor">
						<uui-toggle
							?checked=${this._isElement}
							@change=${(e: CustomEvent) => {
								this.#workspaceContext?.setIsElement((e.target as UUIToggleElement).checked);
							}}
							label=${this.localize.term('contentTypeEditor_elementType')}></uui-toggle>
					</div>
				</umb-property-layout>
			</uui-box>
			<uui-box headline=${this.localize.term('contentTypeEditor_historyCleanupHeading')}>
				<umb-property-layout
					alias="HistoryCleanup"
					label=${this.localize.term('contentTypeEditor_historyCleanupHeading')}>
					<div slot="description">
						<umb-localize key="contentTypeEditor_historyCleanupDescription"
							>Allow overriding the global history cleanup settings.</umb-localize
						>
					</div>
					<div slot="editor">
						<uui-form-layout-item>
							<uui-toggle
								id="prevent-cleanup"
								label=${this.localize.term('contentTypeEditor_historyCleanupPreventCleanup')}
								.checked=${this._preventCleanup ?? false}
								@change=${this.#onChangePreventCleanup}></uui-toggle>
						</uui-form-layout-item>

						${when(
							!this._preventCleanup,
							() => html`
								<uui-form-layout-item>
									<uui-label slot="label" for="versions-newer-than-days">
										<umb-localize key="contentTypeEditor_historyCleanupKeepAllVersionsNewerThanDays"
											>Keep all versions newer than days</umb-localize
										>
									</uui-label>

									<uui-input
										type="number"
										id="versions-newer-than-days"
										min="0"
										placeholder="7"
										.value=${this._keepAllVersionsNewerThanDays}
										@change=${this.#onChangeKeepAllVersionsNewerThanDays}></uui-input>
								</uui-form-layout-item>

								<uui-form-layout-item>
									<uui-label slot="label" for="latest-version-per-day-days">
										<umb-localize key="contentTypeEditor_historyCleanupKeepLatestVersionPerDayForDays"
											>Keep latest version per day for days</umb-localize
										>
									</uui-label>
									<uui-input
										type="number"
										id="latest-version-per-day-days"
										min="0"
										placeholder="90"
										.value=${this._keepLatestVersionPerDayForDays}
										@change=${this.#onChangeKeepLatestVersionPerDayForDays}></uui-input>
								</uui-form-layout-item>
							`,
						)}
					</div>
				</umb-property-layout>
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
				padding-bottom: var(--uui-size-layout-1); // To enforce some distance to the bottom of the scroll-container.
			}
			uui-box {
				margin-top: var(--uui-size-layout-1);
			}

			uui-label,
			umb-property-editor-ui-number {
				display: block;
			}

			// TODO: is this necessary?
			uui-toggle {
				display: flex;
			}
		`,
	];
}

export default UmbDocumentTypeWorkspaceViewSettingsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-workspace-view-settings': UmbDocumentTypeWorkspaceViewSettingsElement;
	}
}

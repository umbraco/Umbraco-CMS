import { property } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbModalHandler } from '@umbraco-cms/backoffice/modal';
import type { UmbModalExtensionElement } from '@umbraco-cms/backoffice/extensions-registry';

export abstract class UmbModalBaseElement<UmbModalData extends object = object, UmbModalResult = unknown>
	extends UmbLitElement
	implements UmbModalExtensionElement<UmbModalData, UmbModalResult>
{
	@property({ attribute: false })
	modalHandler?: UmbModalHandler<UmbModalData, UmbModalResult>;

	@property({ type: Object, attribute: false })
	data?: UmbModalData;
}

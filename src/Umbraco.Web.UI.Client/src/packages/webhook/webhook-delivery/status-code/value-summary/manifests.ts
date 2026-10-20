import { UMB_WEBHOOK_DELIVERY_STATUS_CODE_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.WebhookDelivery.StatusCode',
		name: 'Webhook Delivery Status Code Value Summary',
		forValueType: UMB_WEBHOOK_DELIVERY_STATUS_CODE_VALUE_TYPE,
		element: () => import('./webhook-delivery-status-code-value-summary.element.js'),
	},
];

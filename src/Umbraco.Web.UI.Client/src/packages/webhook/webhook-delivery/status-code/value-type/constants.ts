export const UMB_WEBHOOK_DELIVERY_STATUS_CODE_VALUE_TYPE = 'Umb.ValueType.WebhookDelivery.StatusCode' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_WEBHOOK_DELIVERY_STATUS_CODE_VALUE_TYPE]: string;
	}
}

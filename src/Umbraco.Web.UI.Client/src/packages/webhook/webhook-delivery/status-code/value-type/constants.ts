export const UMB_WEBHOOK_DELIVERY_STATUS_CODE_VALUE_TYPE = 'Umb.ValueType.WebhookDelivery.StatusCode' as const;

export interface UmbWebhookDeliveryStatusCodeValue {
	/** The status as stored and displayed, e.g. `OK (200)` or `ConnectionError`. */
	label: string;
	/** The numeric HTTP status code, or `null` when the delivery has no HTTP status code (e.g. a connection error). */
	code: number | null;
}

declare global {
	interface UmbValueTypeMap {
		[UMB_WEBHOOK_DELIVERY_STATUS_CODE_VALUE_TYPE]: UmbWebhookDeliveryStatusCodeValue;
	}
}

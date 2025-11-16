using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CollectibleItem : MonoBehaviour, IInteractable {
    [SerializeField] private CollectibleItemSO itemSO;

    public CollectibleItemSO ItemSO {
        get => itemSO;
        set => itemSO = value;
    }

    private ParticleSystem particleSys;
    private UIManager uiManager;
    private AudioSource audioSource;

    private void OnEnable() {
        // Search also in children, so ShineParticle child is found
        particleSys = GetComponentInChildren<ParticleSystem>();
        uiManager = FindFirstObjectByType<UIManager>();
        audioSource = GetComponent<AudioSource>();

        if (particleSys != null && itemSO != null) {
            ApplyRarityVisual(itemSO);
            if (!particleSys.isPlaying)
                particleSys.Play();
        }
    }

    public void Interact(GameObject interactor) {
        var stats = interactor.GetComponent<StatsManager>();
        if (stats == null || !stats.canCollectItems)
            return;

        ApplyEffects(stats);
        PlayPickupUI();
        PlayPickupSfx();

        Destroy(gameObject);
    }

    public Vector3 GetPosition() {
        return transform.position;
    }

    private void ApplyEffects(StatsManager statsManager) {
        if (itemSO == null) return;

        foreach (var effect in itemSO.itemEffects) {
            if (effect != null)
                statsManager.ApplyEffect(effect);
        }
    }

    private void PlayPickupUI() {
        if (uiManager != null)
            uiManager.ShowItemPickup(itemSO);
    }

    private void PlayPickupSfx() {
        if (itemSO.pickupSfx == null)
            return;

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.PlayOneShot(itemSO.pickupSfx);
    }

    private void ApplyRarityVisual(CollectibleItemSO so) {
        if (particleSys == null) return;

        Color rarityColor = so.rarityColor.a > 0f
            ? so.rarityColor
            : GetDefaultRarityColor(so.itemRarity);

        var main = particleSys.main;
        main.startColor = rarityColor;
    }

    private Color GetDefaultRarityColor(ItemRarity rarity) {
        switch (rarity) {
            case ItemRarity.common: return Color.white;
            case ItemRarity.uncommon: return new Color(0.4f, 1f, 0.4f);
            case ItemRarity.rare: return Color.cyan;
            case ItemRarity.epic: return new Color(0.7f, 0.3f, 1f);
            case ItemRarity.legendary: return new Color(1f, 0.5f, 0f);
            case ItemRarity.mythic: return Color.magenta;
            case ItemRarity.unique: return Color.red;
            default: return Color.gray;
        }
    }
}

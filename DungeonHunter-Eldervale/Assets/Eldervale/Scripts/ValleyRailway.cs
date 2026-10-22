using UnityEngine;

public static class ValleyRailway
{
    public static void Build(Transform parent, Material wood, Material metal, Material stone)
    {
        var root = new GameObject("Valley Scenic Railway").transform;
        root.SetParent(parent);

        Vector3 center = new Vector3(45, 0.04f, 15);
        float radiusX = 22f;
        float radiusZ = 30f;
        const int numTies = 60;

        // --- 1. Railway Tracks (Wooden Ties & Iron Rails) ---
        var trackGroup = new GameObject("Railway Tracks").transform;
        trackGroup.SetParent(root);

        for (int i = 0; i < numTies; i++)
        {
            float t = i / (float)numTies * Mathf.PI * 2f;
            Vector3 pos = center + new Vector3(Mathf.Sin(t) * radiusX, 0, Mathf.Cos(t) * radiusZ);
            // Tangent direction
            Vector3 tangent = new Vector3(Mathf.Cos(t) * radiusX, 0, -Mathf.Sin(t) * radiusZ).normalized;
            Quaternion rot = Quaternion.LookRotation(tangent);

            // Wooden tie / sleeper
            var tie = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tie.name = "Rail tie " + i;
            tie.transform.SetParent(trackGroup);
            tie.transform.position = pos + Vector3.up * 0.04f;
            tie.transform.rotation = rot;
            tie.transform.localScale = new Vector3(2.4f, 0.08f, 0.35f);
            tie.GetComponent<Renderer>().sharedMaterial = wood;
            Object.Destroy(tie.GetComponent<Collider>());

            // Left & Right iron rails
            for (int r = -1; r <= 1; r += 2)
            {
                var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rail.name = "Iron rail";
                rail.transform.SetParent(trackGroup);
                rail.transform.position = pos + rot * new Vector3(r * 0.85f, 0.12f, 0);
                rail.transform.rotation = rot;
                rail.transform.localScale = new Vector3(0.12f, 0.14f, (radiusZ * Mathf.PI * 2f / numTies) * 1.1f);
                rail.GetComponent<Renderer>().sharedMaterial = metal;
                Object.Destroy(rail.GetComponent<Collider>());
            }
        }

        // --- 2. Train Station Platform (Stasiun Eldervale) ---
        Vector3 stationPos = center + new Vector3(-radiusX - 3.2f, 0, 0);
        var station = new GameObject("Stasiun Eldervale Platform").transform;
        station.SetParent(root);
        station.position = stationPos;

        // Wooden Platform Deck
        var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.transform.SetParent(station, false);
        platform.transform.localPosition = new Vector3(0, 0.22f, 0);
        platform.transform.localScale = new Vector3(4.5f, 0.44f, 16f);
        platform.GetComponent<Renderer>().sharedMaterial = wood;

        // Platform Roof Canopy with Pillars
        for (int pz = -6; pz <= 6; pz += 4)
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillar.transform.SetParent(station, false);
            pillar.transform.localPosition = new Vector3(-1.8f, 1.8f, pz);
            pillar.transform.localScale = new Vector3(0.24f, 3.2f, 0.24f);
            pillar.GetComponent<Renderer>().sharedMaterial = wood;

            // Station Lantern
            var stantern = new GameObject("Station Lantern").AddComponent<Light>();
            stantern.transform.SetParent(station, false);
            stantern.transform.localPosition = new Vector3(-1.4f, 2.8f, pz);
            stantern.type = LightType.Point;
            stantern.color = new Color(1f, 0.85f, 0.5f);
            stantern.range = 8f;
            stantern.intensity = 1.6f;
        }

        var canopy = GameObject.CreatePrimitive(PrimitiveType.Cube);
        canopy.transform.SetParent(station, false);
        canopy.transform.localPosition = new Vector3(-0.2f, 3.4f, 0);
        canopy.transform.localScale = new Vector3(5.2f, 0.2f, 17f);
        canopy.GetComponent<Renderer>().sharedMaterial = wood;

        // Station Bench & Sign
        var bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bench.transform.SetParent(station, false);
        bench.transform.localPosition = new Vector3(-1.2f, 0.65f, 2f);
        bench.transform.localScale = new Vector3(0.9f, 0.45f, 2.5f);
        bench.GetComponent<Renderer>().sharedMaterial = wood;

        // --- 3. Vintage Steam Train & Carriages ---
        var trainObj = new GameObject("Eldervale Scenic Express");
        trainObj.transform.SetParent(root);
        var ride = trainObj.AddComponent<TrainRide>();
        ride.Center = center;
        ride.RadiusX = radiusX;
        ride.RadiusZ = radiusZ;

        // Steam Locomotive Engine
        var engine = new GameObject("Locomotive Engine").transform;
        engine.SetParent(trainObj.transform, false);

        // Boiler (cylinder)
        var boiler = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        boiler.transform.SetParent(engine, false);
        boiler.transform.localPosition = new Vector3(0, 1.25f, 0.5f);
        boiler.transform.localRotation = Quaternion.Euler(90, 0, 0);
        boiler.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
        boiler.GetComponent<Renderer>().sharedMaterial = metal;
        Object.Destroy(boiler.GetComponent<Collider>());

        // Driver Cab
        var cab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cab.transform.SetParent(engine, false);
        cab.transform.localPosition = new Vector3(0, 1.65f, -1.35f);
        cab.transform.localScale = new Vector3(1.9f, 2.3f, 1.6f);
        cab.GetComponent<Renderer>().sharedMaterial = wood;
        Object.Destroy(cab.GetComponent<Collider>());

        // Chimney Smokestack
        var chimney = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        chimney.transform.SetParent(engine, false);
        chimney.transform.localPosition = new Vector3(0, 2.2f, 1.4f);
        chimney.transform.localScale = new Vector3(0.35f, 0.6f, 0.35f);
        chimney.GetComponent<Renderer>().sharedMaterial = metal;
        Object.Destroy(chimney.GetComponent<Collider>());

        // Glowing Train Headlight
        var headlight = new GameObject("Train Headlight").AddComponent<Light>();
        headlight.transform.SetParent(engine, false);
        headlight.transform.localPosition = new Vector3(0, 1.3f, 2.05f);
        headlight.type = LightType.Spot;
        headlight.range = 22f;
        headlight.spotAngle = 45f;
        headlight.intensity = 2.8f;
        headlight.color = new Color(1f, 0.92f, 0.65f);

        // Cowcatcher front grill
        var grill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grill.transform.SetParent(engine, false);
        grill.transform.localPosition = new Vector3(0, 0.45f, 2.05f);
        grill.transform.localRotation = Quaternion.Euler(25, 0, 0);
        grill.transform.localScale = new Vector3(1.7f, 0.6f, 0.4f);
        grill.GetComponent<Renderer>().sharedMaterial = metal;
        Object.Destroy(grill.GetComponent<Collider>());

        // Passenger Carriage
        var carriage = new GameObject("Passenger Carriage").transform;
        carriage.SetParent(trainObj.transform, false);
        carriage.localPosition = new Vector3(0, 0, -3.8f);

        var cdeck = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cdeck.transform.SetParent(carriage, false);
        cdeck.transform.localPosition = new Vector3(0, 0.55f, 0);
        cdeck.transform.localScale = new Vector3(2.0f, 0.25f, 3.2f);
        cdeck.GetComponent<Renderer>().sharedMaterial = wood;

        var croof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        croof.transform.SetParent(carriage, false);
        croof.transform.localPosition = new Vector3(0, 2.1f, 0);
        croof.transform.localScale = new Vector3(2.1f, 0.15f, 3.4f);
        croof.GetComponent<Renderer>().sharedMaterial = wood;
        Object.Destroy(croof.GetComponent<Collider>());

        // Carriage benches
        for (int b = -1; b <= 1; b += 2)
        {
            var cb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cb.transform.SetParent(carriage, false);
            cb.transform.localPosition = new Vector3(0, 0.9f, b * 0.85f);
            cb.transform.localScale = new Vector3(1.7f, 0.45f, 0.55f);
            cb.GetComponent<Renderer>().sharedMaterial = wood;
            Object.Destroy(cb.GetComponent<Collider>());
        }

        // Interactive Train Ride trigger
        var interactObj = new GameObject("Naik Kereta Interaksi");
        interactObj.transform.SetParent(carriage, false);
        interactObj.transform.localPosition = new Vector3(0, 0.8f, 0);
        var interact = interactObj.AddComponent<Interactable>();
        interact.Kind = UseKind.Rest;
        interact.Label = "Naik Kereta Wisata Lembah (Whistle & Ride)";
        ride.RideInteractable = interact;
    }
}

public sealed class TrainRide : MonoBehaviour
{
    public Vector3 Center;
    public float RadiusX = 22f;
    public float RadiusZ = 30f;
    public float Speed = 0.18f;
    public Interactable RideInteractable;

    float t = 0f;
    bool playerRiding;
    HunterPlayer player;
    float nextWhistle;

    void Update()
    {
        t += Speed * Time.deltaTime;
        if (t > Mathf.PI * 2f) t -= Mathf.PI * 2f;

        Vector3 pos = Center + new Vector3(Mathf.Sin(t) * RadiusX, 0, Mathf.Cos(t) * RadiusZ);
        Vector3 tangent = new Vector3(Mathf.Cos(t) * RadiusX, 0, -Mathf.Sin(t) * RadiusZ).normalized;

        transform.position = pos;
        if (tangent.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(tangent);
        }

        // Periodic train whistle
        if (Time.time > nextWhistle)
        {
            nextWhistle = Time.time + Random.Range(35f, 60f);
            GameAudio.I?.Cue("whistle", transform.position);
        }

        // Check if player boarded
        if (playerRiding && player != null)
        {
            player.transform.position = transform.position + transform.rotation * new Vector3(0, 0.95f, -3.8f);
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                playerRiding = false;
                player = null;
                GameFlow.I?.Note("Turun dari kereta wisata.");
            }
        }
    }

    public void Board(HunterPlayer p)
    {
        playerRiding = true;
        player = p;
        GameAudio.I?.Cue("whistle", transform.position);
        GameFlow.I?.Note("Menaiki Kereta Wisata Lembah! Tekan Lompat untuk turun.");
    }
}
